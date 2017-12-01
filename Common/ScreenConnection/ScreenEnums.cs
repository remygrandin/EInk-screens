using System;

namespace ScreenConnection
{
    //|    7    |    6    |    5    |    4    |    3    |    2    |    1    |    0    |
    //| VPOS PG | VNEG PG |    0    | VDDH PG | VEE  PG |    0    | V3P3 EN |    0    |
    [Flags]
    public enum PowerStatus : byte
    {
        // Source
        VPOS_PG = 0b10000000,
        VNEG_PG = 0b01000000,
        SOURCE_ON = VPOS_PG | VNEG_PG,

        // Gate
        VDDH_PG = 0b00010000,
        VEE_PG = 0b00001000,
        GATE_ON = VDDH_PG | VEE_PG,

        // V3P3
        V3P3_PG = 0b00000010,

        ON = SOURCE_ON | GATE_ON | V3P3_PG,
        OFF = 0b00000000
    }

    public enum PowerStrobe : byte
    {
        Strobe1 = 0,
        Strobe2 = 1,
        Strobe3 = 2,
        Strobe4 = 3
    }

    public struct PowerSequence
    {
        public PowerStrobe VPOS;
        public PowerStrobe VNEG;
        public PowerStrobe VDDH;
        public PowerStrobe VEE;
    }

    public struct PowerUpTiming
    {
        public byte StartToS1;
        public byte S1ToS2;
        public byte S2ToS3;
        public byte S3ToS4;
    }

    public struct PowerDownTiming
    {
        public byte StartToS1;
        public byte S1ToS2;
        public byte S2ToS3;
        public byte S3ToS4;
        public byte Multiplier;
    }
}
