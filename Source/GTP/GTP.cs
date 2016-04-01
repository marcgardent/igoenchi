using System;
using System.Drawing;
using System.Collections.Generic;

namespace IGoEnchi
{
    public abstract class Link
    {
        public abstract void SendCommand(string command);
        public abstract string TryReceiveCommand();
        public abstract void Close();
    }

    public delegate void Action();
    public delegate void CallbackAction(string message, bool failed);
    public delegate void PassAction(bool isBlack);
    public delegate void ResignAction(bool isBlack);
    public delegate void MoveAction(int x, int y, bool isBlack);        
    
    public class GTP
    {
        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        private Link link;
        private CallbackAction responseHandler;        
        private Action<string> empty;
        private DateTime lastResponse;
        
        public bool Closed {get; private set;}
        public int ProtocolVersion {get; private set;}        
        public int BoardSize {get; private set;}
        public int Handicap {get; private set;}		
        
        public event Action SetupCompleted;
        public event Action<string> ErrorReceived;
        public event Action TimeoutOccured;
        public event PassAction PassReceived;
        public event MoveAction MoveReceived;
        public event Action<int> UndoReceived;
        public event ResignAction ResignReceived;
        public event Action<string> ScoreReceived;
        public event Action<IEnumerable<Point>> HandicapReceived;
        public event Action<bool> LoadSGFCompleted;

        public GTP(Link link, int boardSize, int handicap, float komi, int level, string sgfFile)
        {
            this.link = link;            
            empty = s => {};
            lastResponse = DateTime.MaxValue;
            Setup(boardSize, handicap, komi, level, 
                  (String.IsNullOrEmpty(sgfFile) || sgfFile.IndexOf(' ') < 0) ? 
                  	sgfFile : 
                  	('"' + sgfFile + '"'));
        }
        
        private void Send(string command, Action<string> handler, bool callOnFailure)
        {        	  
        	if (!Closed)
        	{
        		lastResponse = DateTime.Now;
        	
        		var action = responseHandler;
        		if (action == null) 
        		{
        			link.SendCommand(command);
        		}      
        	
        		responseHandler = (s, success) =>
        		{
        			responseHandler = null;
        			if (action != null)
        			{
        				action(s, success);
        				Send(command, handler, callOnFailure);
        			}
        			else if (success || callOnFailure)
        			{
        				handler(s);
        			}
        		};
        	}
        }

        private void Send(string command, Action<string> handler)
        {            
            Send(command, handler, false);
        }        
        
        private void Send(string command)
        {            
            Send(command, empty, false);
        }        
        
        private void ReadResponse(string message)
        {
            if (responseHandler != null)
            {
            	responseHandler(message, true);
            }
        }

        private Point CoordsToPoint(string coords)
        {
        	var c = coords[0];
        	var x = c < 'I' ? c - 'A' : c - 'B';
            var y = BoardSize - Convert.ToInt32(coords.Substring(1));
            return new Point(x, y);
        }
        
        private void ReadMove(string message, bool isBlack)
        {
            if (message == "PASS")
            {
                OnPassReceived(isBlack);
            }
            else if (message == "resign")
            {
                OnResignReceived(isBlack);
            }
            else if (message.Length < 2)
            {
                OnErrorReceived("Invalid move received: " + message);
            }
            else
            {
                try
                {
                	var point = CoordsToPoint(message);
                	OnMoveReceived(point.X, point.Y, isBlack);
                }
                catch (Exception)                
                {
                    OnErrorReceived("Invalid move received: " + message);
                }
            }
        }

        private void ReadHandicap(string message)
        {
        	try
        	{
        		var stones = message.Split(new [] {' '});        	
        		var handicap = new List<Point>();
        		foreach (var stone in stones)
        		{
        			handicap.Add(CoordsToPoint(stone));
        		}
        		OnHandicapReceived(handicap);
        	}
        	catch (Exception)
        	{
        		OnErrorReceived("Invalid handicap received: " + message);
        	}
        }
        
        private void ReadSGFLoad(string message)
        {
        	OnLoadSGFCompleted(message == "black" || message == "white");
        }
        
        public bool Waiting
        {
            get {return !Closed && responseHandler != null;}
        }

        private TimeSpan TimeElapsed
        {
            get {return DateTime.Now - lastResponse;}
        }

        private void Setup(int boardSize, int handicap, float komi, int level, string sgfFile)
        {
            if (!Waiting)
            {
                ProtocolVersion = 1;
                if (boardSize < 2 || boardSize > 19)
                {
                    boardSize = 19;
                }
                BoardSize = boardSize;
                
                Send("protocol_version", s => 
                	 {                        
	                    try 
    	                {
        	              	ProtocolVersion = Convert.ToInt32(s);
            	        }
                	    catch (Exception) {}
                     });
                Send("boardsize " + boardSize);
                Send("clear_board");
                if (handicap > 0) 
                {
                	Send("fixed_handicap " + handicap, ReadHandicap);
                }
                Send("komi " + ConfigManager.FloatToString(komi));
                if (!String.IsNullOrEmpty(sgfFile))
                {
                	Send("loadsgf " + sgfFile, ReadSGFLoad, true);
                }
                Send("level " + level, s => OnSetupCompleted(), true);               
            }
        }

        private void SendPlay(string command, Action<string> handler, bool isBlack)
        {        	
            if (!Waiting)
            {
                if (ProtocolVersion == 1)
                {
                    Send("play" + (isBlack ? "black " : "white ") + command, handler);
                }
                else
                {
                    Send("play " + (isBlack ? "B " : "W ") + command, handler);
                }
            }
        }        
        
        public void Play(int x, int y, bool isBlack)
        {
            if (x >= 0 && x < BoardSize &&
                y >= 0 && y < BoardSize)
            {
                var coord = ((char) ((x <= 7 ? 'A' : 'B') + x)).ToString() + 
                             (BoardSize - y).ToString();
                SendPlay(coord, s => OnMoveReceived(x, y, isBlack), isBlack);
            }
        }

        public void Pass(bool isBlack)
        {
            if (!Waiting)
            {
                SendPlay("pass", s => OnPassReceived(isBlack), isBlack);
            }
        }        
                        
        public void RequestMove(bool isBlack)
        {
            if (!Waiting)
            {
                if (ProtocolVersion == 1)
                {
                    Send("genmove_" + (isBlack ? "black" : "white"), 
                         s => ReadMove(s, isBlack));
                }
                else
                {
                    Send("genmove " + (isBlack ? "B" : "W"),
                         s => ReadMove(s, isBlack));
                }
            }
        }

        public bool SupportsUndo
        {
        	get {return ProtocolVersion != 1;}
        }
        
        public void Undo(int count)
        {
            if (!Waiting && ProtocolVersion != 1)
            {
                Send("gg-undo " + count, s => OnUndoReceived(count));
            }
        }

        public void RequestScore()
        {
            if (!Waiting)
            {
                Send("final_score", OnScoreReceived);
            }
        }

        public void Quit()
        {
        	if (!Closed)
        	{
        		link.SendCommand("quit");
        		link.Close();
        		Closed = true;
        	}
        }
        
        public void Update()
        {
            if (Waiting)
            {
                var response = link.TryReceiveCommand();
                if (response != String.Empty)
                {            
                    if (response.StartsWith("?"))
                    {
                        OnErrorReceived(response.Substring(1).Trim());
                    }
                    else if (response.StartsWith("="))
                    {
                        ReadResponse(response.Substring(1).Trim());
                    }
                    lastResponse = DateTime.Now;
                }
                else if (TimeElapsed > Timeout)
                {
                    lastResponse = DateTime.Now;
                    OnTimeoutOccured();
                }
            }
        }

        private void OnSetupCompleted()
        {
            if (SetupCompleted != null)
            {
                SetupCompleted();
            }
        }

        private void OnTimeoutOccured()
        {
            if (TimeoutOccured != null)
            {
                TimeoutOccured();
            }
        }

        private void OnErrorReceived(string message)
        {
            if (ErrorReceived != null)
            {
                ErrorReceived(message);
            }
            if (responseHandler != null)
            {
            	responseHandler(message, false);
            }
        }

        private void OnPassReceived(bool isBlack)
        {
            if (PassReceived != null)
            {
                PassReceived(isBlack);
            }
        }

        private void OnMoveReceived(int x, int y, bool isBlack)
        {
            if (MoveReceived != null)
            {
                MoveReceived(x, y, isBlack);
            }
        }

        private void OnUndoReceived(int moves)
        {
            if (UndoReceived != null)
            {
                UndoReceived(moves);
            }
        }

        private void OnResignReceived(bool isBlack)
        {
            if (ResignReceived != null)
            {
                ResignReceived(isBlack);
            }
        }

        private void OnScoreReceived(string score)
        {
            if (ScoreReceived != null)
            {
                ScoreReceived(score);
            }
        }
        
        private void OnHandicapReceived(IEnumerable<Point> stones)
        {
        	if (HandicapReceived != null && stones != null)
        	{
        		HandicapReceived(stones);
        	}
        }     
        
        private void OnLoadSGFCompleted(bool success)
        {
        	if (LoadSGFCompleted != null)
        	{
        		LoadSGFCompleted(success);
        	}
        }
    }
}