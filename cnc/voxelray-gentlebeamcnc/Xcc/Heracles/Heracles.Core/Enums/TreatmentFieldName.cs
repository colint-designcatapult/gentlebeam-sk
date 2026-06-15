namespace Heracles.Core.Enums
{
    public class GCBNameAttribute : Attribute {
        public string Name { get; set; }
        public GCBNameAttribute(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
        }
    }
    public enum TreatmentFieldName
    {
        [GCBName("+4L2")] Plus4L2 = 1,
        [GCBName("+4L1")] Plus4L1 = 2,
        [GCBName("+4C") ] Plus4C = 3,
        [GCBName("+4R1")] Plus4R1 = 4,
        [GCBName("+4R2")] Plus4R2 = 5,
        [GCBName("+3L3")] Plus3L3 = 6,
        [GCBName("+3L2")] Plus3L2 = 7,
        [GCBName("+3L1")] Plus3L1 = 8,
        [GCBName("+3R1")] Plus3R1 = 9,
        [GCBName("+3R2")] Plus3R2 = 10,
        [GCBName("+3R3")] Plus3R3 = 11,
        [GCBName("+2L3")] Plus2L3 = 12,
        [GCBName("+2L2")] Plus2L2 = 13,
        [GCBName("+2L1")] Plus2L1 = 14,
        [GCBName("+2C") ] Plus2C = 15,
        [GCBName("+2R1")] Plus2R1 = 16,
        [GCBName("+2R2")] Plus2R2 = 17,
        [GCBName("+2R3")] Plus2R3 = 18,
        [GCBName("+1L4")] Plus1L4 = 19,
        [GCBName("+1L3")] Plus1L3 = 20,
        [GCBName("+1L2")] Plus1L2 = 21,
        [GCBName("+1L1")] Plus1L1 = 22,
        [GCBName("+1R1")] Plus1R1 = 23,
        [GCBName("+1R2")] Plus1R2 = 24,
        [GCBName("+1R3")] Plus1R3 = 25,
        [GCBName("+1R4")] Plus1R4 = 26,
        [GCBName("+0L4")] Plus0L4 = 27,
        [GCBName("+0L3")] Plus0L3 = 28,
        [GCBName("+0L2")] Plus0L2 = 29,
        [GCBName("+0L1")] Plus0L1 = 30,
        [GCBName("C")   ] PlusC = 31,
        [GCBName("+0R1")] Plus0R1 = 32,
        [GCBName("+0R2")] Plus0R2 = 33,
        [GCBName("+0R3")] Plus0R3 = 34,
        [GCBName("+0R4")] Plus0R4 = 35,
        [GCBName("-1L4")] Minus1L4 = 36,
        [GCBName("-1L3")] Minus1L3 = 37,
        [GCBName("-1L2")] Minus1L2 = 38,
        [GCBName("-1L1")] Minus1L1 = 39,
        [GCBName("-1R1")] Minus1R1 = 40,
        [GCBName("-1R2")] Minus1R2 = 41,
        [GCBName("-1R3")] Minus1R3 = 42,
        [GCBName("-1R4")] Minus1R4 = 43,
        [GCBName("-2L3")] Minus2L3 = 44,
        [GCBName("-2L2")] Minus2L2 = 45,
        [GCBName("-2L1")] Minus2L1 = 46,
        [GCBName("-2C") ] Minus2C = 47,
        [GCBName("-2R1")] Minus2R1 = 48,
        [GCBName("-2R2")] Minus2R2 = 49,
        [GCBName("-2R3")] Minus2R3 = 50,
        [GCBName("-3L3")] Minus3L3 = 51,
        [GCBName("-3L2")] Minus3L2 = 52,
        [GCBName("-3L1")] Minus3L1 = 53,
        [GCBName("-3R1")] Minus3R1 = 54,
        [GCBName("-3R2")] Minus3R2 = 55,
        [GCBName("-3R3")] Minus3R3 = 56,
        [GCBName("-4L2")] Minus4L2 = 57,
        [GCBName("-4L1")] Minus4L1 = 58,
        [GCBName("-4C") ] Minus4C = 59,
        [GCBName("-4R1")] Minus4R1 = 60,
        [GCBName("-4R2")] Minus4R2 = 61,
    }
}