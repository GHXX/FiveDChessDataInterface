namespace DataInterfaceConsoleTest.Variants;

internal class Variant {
    public static readonly Variant[] Variants = new Variant[]
        {
            new Variant("Standard", 8, "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR:0L:0:0"),
            new Variant("Custom - Royalty War", 8, new string[]{"rnbycbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR:-0L:0:0", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBYCBNR:+0L:0:0"}),
            new Variant("Custom - Defended King's Pawn", 8, "rbnqknbr/pppppppp/8/8/8/8/PPPPPPPP/RBNQKNBR:0L:0:0"),
            new Variant("Custom - Horde Chess", 8, "rnbqkbnr/pppppppp/8/1PP2PP1/PPPPPPPP/PPPPPPPP/PPPPPPPP/PPPPPPPP:0L:0:0"),
            new Variant("Custom - Brawn Horde", 8, "rnbqkbnr/wwwwwwww/8/1WW2WW1/WWWWWWWW/WWWWWWWW/WWWWWWWW/WWWWWWWW:0L:0:0"),
            new Variant("Custom - Peasant Revolt", 8, "1nn1k1n1/4p3/8/8/8/8/PPPPPPPP/4K3:0L:0:0"),
            new Variant("Custom - Mournful Vertigo", 7, new string[]{"2B2R1/4CN1/7/7/3p3/2nbpw1/Wbdrukn:-0L:0:0", "wBDRUKN/2NBPW1/3P3/7/7/4cn1/2b2r1:+0L:0:0"}),
            new Variant("Excessive - Two Regents", 8, "rusyksdr/rnbcqbnr/wwwwwwww/pppppppp/PPPPPPPP/WWWWWWWW/RNBCQBNR/RUSYKSDR:0L:0:0"),
            new Variant("Excessive - Slanted", 8, "kqbnwp2/qbnwp3/bnwp3P/nwp3PW/wp3PWN/p3PWNB/3PWNBQ/2PWNBQK:0L:0:0"),
            new Variant("Excessive - Quadrants", 8, "snbwWBNS/nycwWCYN/bcrwWRCB/wwwwWWWW/WWWWwwww/BQRWwrqb/NKQWwqkn/SNBWwbns:0L:0:0")
        };

    //timelinestrings should be of form fenstring:timeline:turnOffset:(0: white, 1: black)
    //multipleTurns Per timeline to be added later

    public string name;
    public int size;
    public string[] timelines;

    public Variant(string name, int size, string[] timelines) {
        this.name = name;
        this.size = size;
        //add checks for now assume correct
        this.timelines = timelines;
    }

    public Variant(string name, int size, string timeline) : this(name, size, new string[] { timeline }) { }
}
