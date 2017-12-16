# IGoEnchi

## About

The project is fork of IGoEnchi 

> IGoEnchi is an Internet Go Server (IGS) client and SGF editor 
> for Windows Mobile. For more information please visit IGoEnchi 
> homepage at  http://sourceforge.net/projects/igoenchi/

## purpose of fork

* Migrate to github
* Compile under Visual Studio with last DotNet Framework
* Separate of concern and make reusable lib and publish NuGet package.
** IGOEnchi.SmartGameLib
** IGOEnchi.GoGameLogic
** IGOEnchi.GoGameSgf

## IGOEnchi.SmartGameLib Library

Library to read/write Sgf-files https://en.wikipedia.org/wiki/Smart_Game_Format

### Usage

### Read SGF

```
SGFTree excepted = SgfReader.LoadFromStream(stream);
```

See also
* For Advanded usage to consume SGFTree see ```IGOEnchi.GoGameSgf.SgfCompiler``` Class

### Create and Write SGF with SgfBuilder
```
	//Build
    var b = new SgfBuilder();
    b.p("b", "M1")
        .Fork(x => x.p("b", "M2").Next().p("b", "M3"))
        .Fork(x => x.p("c", "M2").Next().p("c", "M3"));

	var sgf = b.ToSGFTree();

	//Save
    using (var file = File.CreateText(path))
    {
		var writer = new SgfWriter(file, true);
        writer.WriteSgfTree(sgf);
	}
```

See also
* SGF Documentation http://www.red-bean.com/sgf/sgf4.html


## IGOEnchi.GoGameSgf Library

Library to read/write gogames with Sgf-files.

### Usage

```
    private static GoGame OpenFile(string path)
    {
        using (var stream = File.OpenRead(path))
        {
            var excepted = SgfReader.LoadFromStream(stream);
            return SgfCompiler.Compile(excepted);
        }
    }

	private static void SaveAsSgf(GoGame gogame, string path)
    {
        var builder = new GoSgfBuilder(gogame);
        var sgf = builder.ToSGFTree();

        using (var file = File.CreateText(path))
        {
            var writer = new SgfWriter(file, true);
            writer.WriteSgfTree(sgf);
        }
    }
```

## IGOEnchi.GoGameLogic Library

Independant library to manipulate gogame.

'''
To consume see 

```
    private static void throughGogameSample(GoGame game)
    {
        game.ToStart();

        do
        {
            var move =game.CurrentNode as GoMoveNode;
                
            if (move!=null)
            {
                //your code
            }
        }
	while ( game.ToNextMove());

	}
```

To construct gogame see   ```IGOEnchi.GoGameSgf.SgfCompiler``` Class