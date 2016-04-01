using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace IGoEnchi 
{
	public class BitPlane
	{
		private BitArray bitArray;
		private byte width;
		private byte height;		
		
		public byte Width
		{
			get {return width;}
		}
		
		public byte Height
		{
			get {return height;}
		}
		
		public bool this[byte xIndex, byte yIndex]
		{
			get 
			{				
				return bitArray.Get(yIndex * width + xIndex);
			}
			
			set
			{				
				bitArray.Set(yIndex * width + xIndex, value);
			}
		}				
		
		public BitPlane(byte width, byte height)
		{			
			this.width = width;
			this.height = height;
			bitArray = new BitArray(width * height);						
		}

		public BitPlane And(BitPlane bitPlane)
		{
			var result = new BitPlane(width, height);
			result.bitArray = this.bitArray.And(bitPlane.bitArray);
			return result;
		}
		
		public BitPlane Or(BitPlane bitPlane)
		{
			var result = new BitPlane(width, height);
			result.bitArray = this.bitArray.Or(bitPlane.bitArray);
			return result;
		}
		
		public BitPlane Xor(BitPlane bitPlane)
		{
			var result = new BitPlane(width, height);
			result.bitArray = this.bitArray.Xor(bitPlane.bitArray);			
			return result;
		}
				
		public static BitPlane operator - (BitPlane leftPlane, BitPlane rightPlane)
		{
			var result = new BitPlane(leftPlane.width, leftPlane.height);
			for (int i = 0; i < leftPlane.bitArray.Count; i++)
			{
				if (leftPlane.bitArray[i] && !rightPlane.bitArray[i])
				{
					result.bitArray[i] = true;
				}
				else
				{
					result.bitArray[i] = false;
				}
			}
			return result;
		}
		
		public bool Empty()
		{
			for (byte i = 0; i < width; i++)
			{
				for (byte j = 0; j < height; j++)
				{
					if (this[i, j] == true)
					{
						return false;
					}
				}
			}
			return true;
		}
		
		public BitPlane(byte size): this(size, size) {}			
		
		public BitPlane Copy()
		{
			BitPlane bitPlaneCopy = new BitPlane(width, height);
			bitPlaneCopy.bitArray = bitArray.Clone() as BitArray;
			return bitPlaneCopy;
		}
	}
		
	public class Stone
	{	
		private byte x;
		private byte y;
		private bool isBlack;
		
		public Stone()
		{
			this.x = 0;
			this.y = 0;
			this.isBlack = true;
		}		
		
		public Stone(byte x, byte y, bool isBlack)
		{
			this.x = x;
			this.y = y;
			this.isBlack = isBlack;
		}		
		
		public Stone(Stone source)
		{
			this.x = source.x;
			this.y = source.y;
			this.isBlack = source.isBlack;
		}	
		
		public Stone OtherColorStone()
		{			
			var stone = new Stone(this);
			stone.isBlack = !isBlack;
			
			return stone;
		}
		
		public bool AtPlaceOf(Stone other)
		{
			return X == other.X && Y == other.Y;
		}
		
		public Stone TopStone()
		{			
			var stone = new Stone(this);
			if (y > 0)
			{				
				stone.y = Convert.ToByte(stone.y - 1);				
			}
			else
			{
				stone.y = Byte.MaxValue;				
			}
			
			return stone;
		}
		
		public Stone LeftStone()
		{			
			var stone = new Stone(this);
			if (x > 0)
			{								
				stone.x = Convert.ToByte(stone.x - 1);
			}
			else
			{
				stone.x = Byte.MaxValue;
			}
			
			return stone;
		}
		
		public Stone BottomStone()
		{			
			var stone = new Stone(this);
			stone.y = Convert.ToByte(stone.y + 1);
			
			return stone;
		}
		
		public Stone RightStone()
		{			
			var stone = new Stone(this);
			stone.x = Convert.ToByte(stone.x + 1);
			
			return stone;
		}
		
		public byte X
		{
			set {x = value;}
			get	{return x;}
		}
		
		public byte Y
		{
			set	{y = value;}
			get	{return y;}
		}
		
		public bool IsBlack
		{
			set	{isBlack = value;}
			get	{return isBlack;}
		}		
		
		public bool SameAs(Stone stone)
		{
			return X == stone.X && Y == stone.Y && IsBlack == stone.IsBlack;
		}		
	}		
	
	public class Board 
	{		
		private BitPlane blackStonesPlane;
		private BitPlane whiteStonesPlane;
		
		private Board(){}
		
		public Board(byte boardSize)
		{
			blackStonesPlane = new BitPlane(boardSize);
			whiteStonesPlane = new BitPlane(boardSize);
		}
			
		public Board Copy()
		{
			var result = new Board();
			result.blackStonesPlane = blackStonesPlane.Copy();
			result.whiteStonesPlane = whiteStonesPlane.Copy();
			return result;
		}
				
		public bool Place(byte x, byte y, bool black)
		{
			if (StoneInBounds(x, y))
			{				
				if (black)
				{
					blackStonesPlane[x, y] = true;
				}
				else
				{
					whiteStonesPlane[x, y] = true;
				}
				return true;
			}
			return false;
		}
		
		public bool Place(Stone stone, bool killGroup)
		{
			if (Place(stone.X, stone.Y, stone.IsBlack))
			{
				if (killGroup)
				{
					KillGroup(stone);
				}
				return true;
			}
			else
			{
				return false;
			}
		}
		
		public bool Place(Stone stone)
		{
			return Place(stone, true);
		}
			
		public bool Remove(byte x, byte y)
		{
			blackStonesPlane[x, y] = false;
			whiteStonesPlane[x, y] = false;
			return true;
		}
		
		public bool Remove(Stone stone)
		{
			return Remove(stone.X, stone.Y);
		}
		
		public bool HasStone(byte x, byte y, bool black)
		{
			if (StoneInBounds(x, y))
			{
				if (black)
				{
					return blackStonesPlane[x, y];
				}
				else
				{
					return whiteStonesPlane[x, y];
				}
			}
			else
			{
				return false;
			}
		}
		
		public bool HasStone(Stone stone)
		{
			if (StoneInBounds(stone))
			{
				if (stone.IsBlack)
				{
					return blackStonesPlane[stone.X, stone.Y];
				}
				else
				{
					return whiteStonesPlane[stone.X, stone.Y];
				}
			}
			else
			{
				return false;
			}
		}
		
		public bool StoneInBounds(byte x, byte y)
		{
			if ((x < blackStonesPlane.Width) && (y < blackStonesPlane.Height))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
				
		public bool StoneInBounds(Stone stone)
		{
			return StoneInBounds(stone.X, stone.Y);
		}
		
		public BitPlane Black
		{
			get {return blackStonesPlane;}			
		}
		
		public BitPlane White
		{
			get {return whiteStonesPlane;}			
		}
			
		public byte Size
		{
			get {return blackStonesPlane.Width;}
		}
		
		public bool GroupIsDead(Stone stone)
		{			
			if (HasStone(stone))			
			{				
				var bitPlane = new BitPlane(blackStonesPlane.Width,
				                            blackStonesPlane.Height);
				
				var stonesToCheck = new List<Stone>();								
				stonesToCheck.Add(stone);
				
				while (stonesToCheck.Count > 0)
				{						
					var thisStone = stonesToCheck[0];
					if (!bitPlane[thisStone.X, thisStone.Y])
					{
						bitPlane[thisStone.X, thisStone.Y] = true;
										
						var nextStone = thisStone.TopStone();
						if (HasStone(nextStone))
						{
							stonesToCheck.Add(nextStone);
						}
						else
						{
							if (StoneInBounds(nextStone.OtherColorStone()) &&
							    !HasStone(nextStone.OtherColorStone()))
							{
								return false;
							}
						}
					
						nextStone = thisStone.LeftStone();
						if (HasStone(nextStone))
						{
							stonesToCheck.Add(nextStone);
						}
						else
						{
							if (StoneInBounds(nextStone.OtherColorStone()) &&
							    !HasStone(nextStone.OtherColorStone()))
							{
								return false;
							}
						}
					
						nextStone = thisStone.BottomStone();
						if (HasStone(nextStone))
						{
							stonesToCheck.Add(nextStone);
						}
						else
						{
							if (StoneInBounds(nextStone.OtherColorStone()) &&
							    !HasStone(nextStone.OtherColorStone()))
							{
								return false;
							}
						}
					
						nextStone = thisStone.RightStone();
						if (HasStone(nextStone))
						{
							stonesToCheck.Add(nextStone);
						}
						else
						{
							if (StoneInBounds(nextStone.OtherColorStone()) &&
							    !HasStone(nextStone.OtherColorStone()))
							{
								return false;
							}
						}
					
						stonesToCheck.Remove(thisStone);
					}
					else
					{
						stonesToCheck.Remove(thisStone);
					}
				}				
				
				return true;
			}	
			else
			{
				return false;
			}		
		}			
		
		private void RemoveGroup(Stone stone)
		{					
			if (!HasStone(stone))
			{
				throw (new Exception("Invalid group token"));
			}
			else
			{
				var stonesToCheck = new List<Stone>();
				
				stonesToCheck.Add(stone);
				
				while (stonesToCheck.Count > 0)
				{
					var thisStone = stonesToCheck[0];
					
					Remove(thisStone);
					
					var nextStone = thisStone.TopStone();
					if (HasStone(nextStone))
					{
						stonesToCheck.Add(nextStone);
					}						
					
					nextStone = thisStone.LeftStone();
					if (HasStone(nextStone))
					{
						stonesToCheck.Add(nextStone);
					}		
					
					nextStone = thisStone.BottomStone();
					if (HasStone(nextStone))
					{
						stonesToCheck.Add(nextStone);
					}
				
					nextStone = thisStone.RightStone();
					if (HasStone(nextStone))
					{
						stonesToCheck.Add(nextStone);
					}
					
					stonesToCheck.Remove(thisStone);
				}				
			}
		}				
		
		public void KillGroup(Stone stone)
		{
			if (GroupIsDead(stone.TopStone().OtherColorStone()))
			{
				RemoveGroup(stone.TopStone().OtherColorStone());
			}
			
			if (GroupIsDead(stone.LeftStone().OtherColorStone()))
			{
				RemoveGroup(stone.LeftStone().OtherColorStone());
			}
			
			if (GroupIsDead(stone.BottomStone().OtherColorStone()))
			{
				RemoveGroup(stone.BottomStone().OtherColorStone());
			}
			
			if (GroupIsDead(stone.RightStone().OtherColorStone()))
			{
				RemoveGroup(stone.RightStone().OtherColorStone());
			}
		}
	}
	
	public abstract class GoNode 
	{
		private List<GoNode> childNodes; 
		private GoNode defaultChildNode;
		private GoNode parentNode;
		private string comment;	
		private BoardMarkup markup;
		protected Board board;		
		
		public BoardMarkup Markup
		{
			get {return markup;}			
		}
		
		public List<GoNode> ChildNodes
		{
			get	{return childNodes;}
		}	
		
		public GoNode DefaultChildNode
		{
			get	{return defaultChildNode;}
		}					
		
		public GoNode ParentNode
		{
			get	{return parentNode;}
		}
			
		public string Comment
		{
			get	{return comment;}
			set {comment = value;}
		}										
		
		public Board BoardCopy
		{
			get 
			{
				if (board != null)
				{
					return board.Copy();
				}
				else
				{
					return CopyParentBoard();
				}
			}
		}
		
		public byte BoardSize
		{
			get
			{
				if (board != null)
				{
					return board.Size;
				}
				else
				{
					return parentNode.BoardSize;
				}
			}
		}
				
		protected virtual Board CopyParentBoard()
		{
			if (parentNode.board != null)
			{
				return parentNode.board.Copy();
			}
			else
			{
				return parentNode.CopyParentBoard();
			}
		}				
		
		public GoNode(GoNode parentNode, bool makeBoards)
		{
			childNodes = new List<GoNode>();
			if (parentNode != null)
			{
				this.parentNode = parentNode;
				if (makeBoards)
				{
					board = CopyParentBoard();
				}
			}
			else
			{				
				this.parentNode = null;
			}
		}						
		
		public GoNode GetNextSibling()
		{
			if (parentNode == null)
			{
				return this;
			}
			else
			{
				var index = parentNode.childNodes.IndexOf(parentNode.defaultChildNode) + 1;				
				return parentNode.GetChild(index);
			}
		}
		
		public GoNode GetPreviousSibling()
		{
			if (parentNode == null)
			{
				return this;
			}
			else
			{
				var index = parentNode.childNodes.IndexOf(parentNode.defaultChildNode) - 1;
				return parentNode.GetChild(index);				
			}
		}				
		
		public bool Exists(Predicate<GoNode> p)
		{
			if (p(this))
			{
				return true;
			}
			return childNodes.Exists(c => Exists(p));
		}
		
		public bool HasChildren
		{
			get {return childNodes.Count > 0;}
		}
		
		public GoNode GetChild(int index)
		{
			if (index < 0)
			{
				index = 0;
			}
			if (index >= childNodes.Count)
			{
				index = childNodes.Count - 1;
			}
				
			defaultChildNode = childNodes[index] as GoNode;
			return defaultChildNode;
		}
		
		public GoNode AddNode(GoNode nextNode)
		{
			childNodes.Add(nextNode);			
			defaultChildNode = nextNode;
			
			return nextNode;
		}

		public void RemoveNode(GoNode node)
		{
			childNodes.Remove(node);			
			if (node == defaultChildNode)
			{				
				defaultChildNode = 
					childNodes.Count > 0 ? childNodes[0] :
										   null;
			}
		}
		
		public void EnsureMarkup()
		{
			if (markup == null)
			{
				markup = new BoardMarkup(BoardSize);
			}
		}
		
		public void MarkDead(Stone stone)
		{
			EnsureMarkup();
			markup.EnsureDeadStones();
			
			markup.DeadStones[stone.X, stone.Y] = true;
						
			stone.IsBlack = board.Black[stone.X, stone.Y];
			
			Stone newStone = stone.LeftStone();
			if (board.HasStone(newStone) && !markup.DeadStones[newStone.X, newStone.Y])
			{					
				MarkDead(newStone);				
			}		
			newStone = stone.TopStone();
			if (board.HasStone(newStone) && !markup.DeadStones[newStone.X, newStone.Y])
			{				
				MarkDead(newStone);				
			}
			newStone = stone.RightStone();
			if (board.HasStone(newStone) && !markup.DeadStones[newStone.X, newStone.Y])
			{				
				MarkDead(newStone);				
			}
			newStone = stone.BottomStone();
			if (board.HasStone(newStone) && !markup.DeadStones[newStone.X, newStone.Y])
			{				
				MarkDead(newStone);				
			}		
		}
	}
	
	public class GoRootNode: GoSetupNode
	{				
		public GoRootNode(byte boardSize): base(null)
		{
			board = new Board(boardSize);
		}
		
		protected override Board CopyParentBoard()
		{
			return board.Copy();
		}
	}
	
	public class GoSetupNode: GoNode
	{
		private bool? blackToPlay;
		
		public GoSetupNode(GoNode parentNode): base(parentNode, true){}
		
		public bool? BlackToPlay
		{
			get {return blackToPlay;}
			set {blackToPlay = value;}
		}
		
		public void SetPlayer(bool black)
		{
			blackToPlay = black;
		}
		
		public void SetHandicap(byte count)
		{			
			switch (board.Size)
			{
				case 19: 
					SetHandicapFor19(count);
					break;
				case 13: 
					SetHandicapFor13(count);
					break;
				case 9: 
					SetHandicapFor9(count);
					break;
			}
		}
		
		public void SetHandicapFor19(byte count)
		{
			switch (count)
			{
				case 2:
					AddStone(new Stone(3, 15, true));
					AddStone(new Stone(15, 3, true));
					break;
				case 3:
					SetHandicapFor19(2);
					AddStone(new Stone(3, 3, true));
					break;
				case 4:
					SetHandicapFor19(3);
					AddStone(new Stone(15, 15, true));
					break;
				case 5:
					SetHandicapFor19(4);
					AddStone(new Stone(9, 9, true));
					break;
				case 6:
					SetHandicapFor19(4);
					AddStone(new Stone(3, 9, true));
					AddStone(new Stone(15, 9, true));
					break;
				case 7:
					SetHandicapFor19(6);
					AddStone(new Stone(9, 9, true));					
					break;
				case 8:
					SetHandicapFor19(6);
					AddStone(new Stone(9, 15, true));
					AddStone(new Stone(9, 3, true));
					break;
				case 9:
					SetHandicapFor19(8);
					AddStone(new Stone(9, 9, true));
					break;
			}
		}
		
		public void SetHandicapFor13(byte count)
		{
			switch (count)
			{
				case 2:
					AddStone(new Stone(3, 9, true));
					AddStone(new Stone(9, 3, true));
					break;
				case 3:
					SetHandicapFor13(2);
					AddStone(new Stone(3, 3, true));
					break;
				case 4:
					SetHandicapFor13(3);
					AddStone(new Stone(9, 9, true));
					break;
				case 5:
					SetHandicapFor13(4);
					AddStone(new Stone(6, 6, true));
					break;
				case 6:
					SetHandicapFor13(4);
					AddStone(new Stone(3, 6, true));
					AddStone(new Stone(9, 6, true));
					break;
				case 7:
					SetHandicapFor13(6);
					AddStone(new Stone(6, 6, true));					
					break;
				case 8:
					SetHandicapFor13(6);
					AddStone(new Stone(6, 9, true));
					AddStone(new Stone(6, 3, true));
					break;
				case 9:
					SetHandicapFor13(8);
					AddStone(new Stone(6, 6, true));
					break;
			}
		}
		
		public void SetHandicapFor9(byte count)
		{
			switch (count)
			{
				case 2:
					AddStone(new Stone(2, 6, true));
					AddStone(new Stone(6, 2, true));
					break;
				case 3:
					SetHandicapFor9(2);
					AddStone(new Stone(2, 2, true));
					break;
				case 4:
					SetHandicapFor9(3);
					AddStone(new Stone(6, 6, true));
					break;
				case 5:
					SetHandicapFor9(4);
					AddStone(new Stone(4, 4, true));
					break;
				case 6:
					SetHandicapFor9(4);
					AddStone(new Stone(2, 4, true));
					AddStone(new Stone(6, 4, true));
					break;
				case 7:
					SetHandicapFor9(6);
					AddStone(new Stone(4, 4, true));					
					break;
				case 8:
					SetHandicapFor9(6);
					AddStone(new Stone(4, 6, true));
					AddStone(new Stone(4, 2, true));
					break;
				case 9:
					SetHandicapFor9(8);
					AddStone(new Stone(4, 4, true));
					break;
			}
		}
		
		public void AddStone(Stone stone)
		{
			board.Place(stone, false);			
		}
		
		public void RemoveStone(Stone stone)
		{
			board.Remove(stone);
		}
	}
	
	public class GoMoveNode: GoNode
	{
		private Stone stone;				
		
		public GoMoveNode(GoNode parentNode, Stone stone, bool makeBoard): base(parentNode, makeBoard)
		{	
			if (board != null)
			{
				board.Place(stone);								
			}
			this.stone = stone;												
		}				
		
		override protected Board CopyParentBoard()
		{
			var result = base.CopyParentBoard();
			if (stone != null)
			{
				result.Place(stone);				
			}
			return result;
		}
		
		public Stone Stone
		{
			get {return stone;}
		}
	}
	
	public struct TextLabel
	{
		public readonly byte X;
		public readonly byte Y;
		public readonly string Text;
		
		public TextLabel(byte x, byte y, string text)
		{
			X = x;
			Y = y;
			Text = text;
		}
	}
	
	public enum MarkType {Square, Circle, Triangle, Mark, Selected};
	
	public struct Mark
	{
		public readonly byte X;
		public readonly byte Y;
		public readonly MarkType MarkType;
		
		public Mark(byte x, byte y, MarkType markType)
		{
			X = x;
			Y = y;
			MarkType = markType;
		}
	}
	
	public class BoardMarkup
	{		
		private byte boardSize;
		private Board territory;
		private BitPlane deadStones;
		private List<TextLabel> labels;
		private List<Mark> marks;

		public Board Territory
		{
			get {return territory;}
		}
		
		public BitPlane DeadStones
		{
			get {return deadStones;}
		}
		
		public List<TextLabel> Labels
		{
			get {return labels;}
		}
		
		public List<Mark> Marks
		{
			get {return marks;}
		}
		
		public BoardMarkup(byte boardSize) 
		{
			labels = new List<TextLabel>();
			marks = new List<Mark>();
			this.boardSize = boardSize;
		}
		
		public void EnsureTerritory()
		{
			if (territory == null)
			{
				territory = new Board(boardSize);
			}
		}
		
		public void FreeTerritory()
		{
			territory = null;
		}
				
		public void EnsureDeadStones()
		{
			if (deadStones == null)
			{
				deadStones = new BitPlane(boardSize);
			}
		}
		
		public void FreeDeadStones()
		{
			deadStones = null;
		}
	}
	
	public class GameInfo
	{
		private string blackPlayer;
		private string whitePlayer;
		private int handicap;
		private float komi;
		
		public GameInfo()
		{
			blackPlayer = "black";
			whitePlayer = "white";
			handicap = 0;
			komi = 0;
		}

		public GameInfo(string blackPlayerName, string whitePlayerName, int handiInt, float komiFloat)
		{
			blackPlayer = blackPlayerName;
			whitePlayer = whitePlayerName;
			handicap = handiInt;
			komi = komiFloat;
		}

		
		public string BlackPlayer
		{
			get {return blackPlayer;}
			set {blackPlayer = value;}
		}
		
		public string WhitePlayer
		{
			get {return whitePlayer;}
			set {whitePlayer = value;}
		}
		
		public int Handicap
		{
			get {return handicap;}
			set {handicap = value;}			
		}
		
		public float Komi
		{
			get {return komi;}
			set {komi = value;}
		}
	}
	
	public enum Variants  
	{
		None,
		Siblings,
		Children		
	}
	
	public class GoGame
	{			
		private GoNode currentNode;
		private int moveNumber;
		private GameInfo gameInfo; 
		private bool updateBoard = true;
		private Variants showVariants;
		private Board board;			
		
		public GoGame(byte boardSize) 
		{
			currentNode = new GoRootNode(boardSize);
			moveNumber = 0;
			gameInfo = new GameInfo();			
			showVariants = Variants.Siblings;
			
			board = new Board(boardSize);
		}
		
		public GameInfo Info
		{
			get {return gameInfo;}
			set {gameInfo = value;}
		}
		
		public int MoveNumber
		{
			get	{return moveNumber;}
		}
		
		public byte BoardSize
		{
			get	{return board.Size;}
		}
		
		public string CurrentComment
		{
			get	{return currentNode.Comment;}
		}
		
		public GoNode CurrentNode
		{
			get	{return currentNode;}
		}
		
		public GoNode RootNode
		{
			get 
			{
				var node = currentNode;
				while (node.ParentNode != null)
				{
					node = node.ParentNode;
				}
				return node;
			}
		}
		
		public bool UpdateBoard
		{
			get {return updateBoard;}
			set {updateBoard = value;}
		}
		
		public Variants ShowVariants
		{
			get {return showVariants;}
			set {showVariants = value;}
		}
		
		public int[] Path()
		{
			var path = new int[moveNumber];
			var node = currentNode;
			var index = moveNumber - 1;
			while (index >= 0)
			{
				var parent = node.ParentNode;
				path[index] = parent.ChildNodes.IndexOf(node);
				node = parent;				
				index -= 1;
			}
			return path;
		}
		
		public GoGame Copy()
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			SGFWriter.SaveGame(this, writer);
			writer.Flush();
			stream.Position = 0;
			var game = SGFCompiler.Compile(SGFLoader.LoadFromStream(stream));			
			stream.Close();
			
			game.ToMove(0);
			foreach (var index in Path())
			{
				game.currentNode = game.currentNode.GetChild(index);
				game.moveNumber = game.moveNumber + 1;
			}			       
			game.Update();
			
			return game;
		}
		
		public void Update()
		{
			board = currentNode.BoardCopy;
		}
			
		public bool CanPlace(Stone stone)
		{
			if (board.HasStone(stone) || board.HasStone(stone.OtherColorStone()))
			{
				return false;
			}
			else
			{
				var canPlace = true;
				board.Place(stone);
				if (board.GroupIsDead(stone))
				{
				    canPlace = false;	
				}
				board.Remove(stone);
				return canPlace;
			}
		}
		
		public bool BlackToPlay()
		{
			if (CurrentNode is GoSetupNode)
			{
				return (CurrentNode as GoSetupNode).BlackToPlay ?? true;							
			}
			else 			
			{
				return !(CurrentNode as GoMoveNode).Stone.IsBlack;				
			}			
		}
		
		public void AddStone(Stone stone)
		{
			GoSetupNode node = null;
			if (currentNode is GoSetupNode)
			{
				node = currentNode as GoSetupNode;				
			}
			else
			{
				node = new GoSetupNode(CurrentNode);				
				AddNode(node);				
			}
			node.RemoveStone(stone);
			node.AddStone(stone);				
			
			board.Remove(stone);
			board.Place(stone, false);
		}
		
		public void RemoveStone(Stone stone)
		{
			if (currentNode is GoSetupNode)
			{
				(currentNode as GoSetupNode).RemoveStone(stone);
				board.Remove(stone);
			}
			else
			{
				var node = new GoSetupNode(CurrentNode);				
				AddNode(node);
				node.RemoveStone(stone);
				board.Remove(stone);
			}			
		}
		
		public bool Continue(Stone stone)
		{
			var nodes = CurrentNode.ChildNodes.FindAll(
				delegate (GoNode node)
				{
					return (node is GoMoveNode) &&
						(node as GoMoveNode).Stone.SameAs(stone);
				});
			
			if (nodes.Count > 0)
			{
				ToMove(nodes[0]);
				return true;
			}
			else
			{
				return false;
			}
		}
		
		public void ToMove(GoNode node)
		{
			foreach (GoNode child in currentNode.ChildNodes)
			{
				if (child == node)
				{
					currentNode = node;
					moveNumber += 1;
					if (updateBoard)
					{
						board = currentNode.BoardCopy;
					}
				}
			}
		}
		
		public void ToMove(int moveNumber)
		{
			if (this.moveNumber > moveNumber)
			{
				while (this.moveNumber > moveNumber)
				{
					ToPreviousMove(false);
				}				
			}
			else
			{
				if (this.moveNumber < moveNumber)
				{
					while (this.moveNumber < moveNumber)
					{
						ToNextMove(false);
					}				
				}	
			}			
			if (updateBoard)
			{
				board = currentNode.BoardCopy;
			}
		}
				
		public void AddNode(GoNode node)
		{
			currentNode = currentNode.AddNode(node);
			moveNumber = moveNumber + 1;
			if (updateBoard)
			{
				board = currentNode.BoardCopy;
			}
		}
				
		public void PlaceStone(Stone stone)
		{
			PlaceStone(stone, true);
		}
		
		public void PlaceStone(Stone stone, bool storeBoard)
		{
			currentNode = currentNode.AddNode(new GoMoveNode(currentNode, stone, storeBoard));
			moveNumber = moveNumber + 1;
			if (updateBoard)
			{
				board.Place(stone);				
			}
		}				
		
		public void SetHandicap(byte count)
		{
			if (currentNode is GoRootNode)
			{
				currentNode = currentNode.AddNode(new GoSetupNode(currentNode));
				(currentNode as GoSetupNode).SetHandicap(count);
				moveNumber = moveNumber + 1;
				board = currentNode.BoardCopy;
			}
		}
		
		public void CommentNode(string comment)
		{
			currentNode.Comment = comment;
		}
				
		public bool ToPreviousMove(bool updateBoard)
		{			
			if (currentNode.ParentNode != null)
			{					
				currentNode = currentNode.ParentNode;
				moveNumber = moveNumber - 1;
				if (updateBoard)
				{
					board = currentNode.BoardCopy;
				}
				return true;
			}
			else
			{
				return false;
			}			
		}
		
		public bool ToPreviousMove()
		{
			return ToPreviousMove(true);		
		}
		
		public bool ToNextMove(bool updateBoard)
		{
			if (currentNode.DefaultChildNode != null)
			{
				currentNode = currentNode.DefaultChildNode;
				moveNumber = moveNumber + 1;
				if (updateBoard)
				{
					if (currentNode is GoMoveNode)
					{
						board.Place((currentNode as GoMoveNode).Stone);						
					}
					else
					{
						board = currentNode.BoardCopy;
					}
				}
				return true;
			}
			else
			{
				return false;
			}			
		}
		
		public bool ToNextMove()
		{	
			return ToNextMove(true);
		}
	
		public void ToPreviousVariation()
		{		
			currentNode = currentNode.GetPreviousSibling();
			board = currentNode.BoardCopy;
		}
		
		public void ToNextVariation()
		{	
			currentNode = currentNode.GetNextSibling();
			board = currentNode.BoardCopy;
		}		
				
		public void Render(GameRenderer renderer)
		{							
			renderer.ClearBoard(board.Size);
									
			for (byte x = 0; x < board.Size; x++)
			{
				for (byte y = 0; y < board.Size; y++)
				{					
					if (board.Black[x, y])
					{
						renderer.DrawStone(x, y, true);						
					}
					else if (board.White[x, y])
					{
						renderer.DrawStone(x, y, false);
					}					
				}
			}
			
			if (currentNode is GoMoveNode)
			{
				var stone = (currentNode as GoMoveNode).Stone;
				
				renderer.DrawMoveMark(stone.X, stone.Y, board.HasStone(stone.X, stone.Y, false));
				if (showVariants == Variants.Siblings)
				{
					currentNode.ParentNode.ChildNodes.
						ForEach(n => RenderShadow(renderer, n));
				}
			}
			
			if (showVariants == Variants.Children)
			{
				currentNode.ChildNodes.
					ForEach(n => RenderShadow(renderer, n));				
			}
						
			if (currentNode.Markup != null)
			{
				RenderMarkup(renderer, currentNode.Markup);				
			}
			
			if (currentNode is GoMoveNode)
			{
				var stone = (currentNode as GoMoveNode).Stone;
				if (stone.X > 19)
				{
					renderer.DrawMessage((stone.IsBlack ? "Black" : "White")
					                     + " passes");
				}
			}
		}
		
		private void RenderShadow(GameRenderer renderer, GoNode node)
		{
			if ((node != currentNode) && (node is GoMoveNode))
			{
				var nodeStone = (node as GoMoveNode).Stone;
				renderer.DrawShadow(nodeStone.X, nodeStone.Y, nodeStone.IsBlack);							
			}			
		}
		
		private void RenderMarkup(GameRenderer renderer, BoardMarkup markup)
		{
			foreach (TextLabel label in markup.Labels)
			{				
				renderer.DrawString(label.X, label.Y, label.Text,
				                    board.HasStone(label.X, label.Y, false));
			}
			
			foreach (Mark mark in markup.Marks)
			{
				switch (mark.MarkType)
				{
					case MarkType.Square:
						renderer.DrawSquare(mark.X, mark.Y, board.HasStone(mark.X, mark.Y, false));
						break;
					case MarkType.Circle:
						renderer.DrawCircle(mark.X, mark.Y, board.HasStone(mark.X, mark.Y, false));
						break;
					case MarkType.Triangle:
						renderer.DrawTriangle(mark.X, mark.Y, board.HasStone(mark.X, mark.Y, false));
						break;
					case MarkType.Mark:
						renderer.DrawMark(mark.X, mark.Y, board.HasStone(mark.X, mark.Y, false));
						break;
					case MarkType.Selected:
						renderer.DrawSelected(mark.X, mark.Y, board.HasStone(mark.X, mark.Y, false));
						break;
				}
			}
			
			if (markup.Territory != null)
			{
				for (byte x = 0; x < markup.Territory.Black.Width; x++)
				{
					for (byte y = 0; y < markup.Territory.Black.Width; y++)
					{					
						if (markup.Territory.Black[x, y])
						{
							renderer.DrawTerritory(x, y, true);
						}					
					}
				}		
				
				for (byte x = 0; x < markup.Territory.White.Width; x++)
				{
					for (byte y = 0; y < markup.Territory.White.Width; y++)
					{
						if (markup.Territory.White[x, y])
						{
							renderer.DrawTerritory(x, y, false);
						}					
					}
				}		
			}
			
			if (markup.DeadStones != null)
			{
				for (byte x = 0; x < markup.DeadStones.Width; x++)
				{
					for (byte y = 0; y < markup.DeadStones.Width; y++)
					{
						if (markup.DeadStones[x, y])
						{
							renderer.DrawMark(x, y, board.HasStone(x, y, false));
						}					
					}
				}			
			}
		}
	}
}
