namespace Ulid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    [StructLayout(LayoutKind.Sequential)]
    public struct Ulid : IComparable
    {
        public static readonly Ulid Empty = new Ulid();
        private static Random generator = new Random();

        #region constructing
        public Ulid(byte[] data)
        {
            if (data.Length != 16)
            {
                throw new ArgumentException("Data must be a byte array of length 16", nameof(data));
            }

            _timeLow = ((uint)data[3] << 24) | ((uint)data[2] << 16) | ((uint)data[1] << 8) | data[0];
            _timeHigh = (ushort)(((uint)data[5] << 8) | data[4]);

            _randomA = (ushort)(((uint)data[7] << 8) | data[6]);
            _randomB = ((uint)data[11] << 24) | ((uint)data[10] << 16) | ((uint)data[9] << 8) | data[8];
            _randomC = ((uint)data[15] << 24) | ((uint)data[14] << 16) | ((uint)data[13] << 8) | data[12];
        }

        public Ulid(uint timeLow, ushort timeHigh, ushort randomA, uint randomB, uint randomC)
        {
            _timeLow = timeLow;
            _timeHigh = timeHigh;

            _randomA = randomA;
            _randomB = randomB;
            _randomC = randomC;
        }

        public Ulid(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            this = Ulid.Empty;

            Ulid result;
            if (TryParse(str, out result))
            {
                this = result;
            }
            else
            {
                throw new ArgumentException("Invalid Ulid", nameof(str));
            }
        }

        public static Ulid New()
        {
            var milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var timeLow = (uint)(milliseconds & Mask(17, 32));
            var timeHigh = (ushort)(milliseconds & Mask(1, 16));

            var randomA = (ushort)generator.Next(ushort.MinValue, ushort.MaxValue);
            var randomB = NextUint();
            var randomC = NextUint();

            return new Ulid(timeLow, timeHigh, randomA, randomB, randomC);
        }
        #endregion

        private static long Mask(byte start, byte length)
        {
            return ((1 << length) - 1) << start;
        }

        private static uint NextUint()
        {
            return (uint)(generator.Next(1 << 30)) << 2 | (uint)(generator.Next(1 << 2));
        }

        #region fields
        private uint _timeLow;
        private ushort _timeHigh;

        private ushort _randomA;
        private uint _randomB;
        private uint _randomC;
        #endregion fields

        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is Ulid))
            {
                throw new ArgumentException("Arugment must be Ulid");
            }

            Ulid other = (Ulid)value;
            if (this._timeHigh != other._timeHigh)
            {
                return Compare(this._timeHigh, other._timeHigh);
            }
            if (this._timeLow != other._timeLow)
            {
                return Compare(this._timeLow, other._timeLow);
            }
            if (this._randomA != other._randomA)
            {
                return Compare(this._randomA, other._randomA);
            }
            if (this._randomB != other._randomB)
            {
                return Compare(this._randomB, other._randomB);
            }
            if (this._randomC != other._randomC)
            {
                return Compare(this._randomC, other._randomC);
            }

            return 0;
        }

        private int Compare(uint me, uint them)
        {
            if (me < them)
            {
                return -1;
            }
            return 1;
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public byte[] ToByteArray()
        {
            byte[] arr = new byte[16];

            arr[0] = (byte)(_timeLow);
            arr[1] = (byte)(_timeLow >> 8);
            arr[2] = (byte)(_timeLow >> 16);
            arr[3] = (byte)(_timeLow >> 24);

            arr[4] = (byte)(_timeHigh);
            arr[5] = (byte)(_timeHigh >> 8);


            arr[6] = (byte)(_randomA);
            arr[7] = (byte)(_randomA >> 8);

            arr[8] = (byte)(_randomB);
            arr[9] = (byte)(_randomB >> 8);
            arr[10] = (byte)(_randomB >> 16);
            arr[11] = (byte)(_randomB >> 24);

            arr[12] = (byte)(_randomC);
            arr[13] = (byte)(_randomC >> 8);
            arr[14] = (byte)(_randomC >> 16);
            arr[15] = (byte)(_randomC >> 24);

            return arr;
        }

        #region parse
        public static Ulid Parse(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            throw new NotImplementedException();
        }

        public static bool TryParse(string input, out Ulid result)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region equality

        public override int GetHashCode()
        {
            unchecked
            {
                const int hashBase = 24391;
                const int multiplier = 1481;

                long hash = hashBase;
                hash = (hash * multiplier) ^ _timeHigh;
                hash = (hash * multiplier) ^ _timeLow;
                hash = (hash * multiplier) ^ _randomA;
                hash = (hash * multiplier) ^ _randomB;
                hash = (hash * multiplier) ^ _randomC;

                return (int)hash; //Might wrap
            }
        }

        public override bool Equals(Object o)
        {
            if (o == null || !(o is Ulid))
            {
                return false;
            }
            var other = (Ulid)o;

            return Equals(other);
        }

        public bool Equals(Ulid other)
        {
            if (this._timeHigh != other._timeHigh)
            {
                return false;
            }
            if (this._timeLow != other._timeLow)
            {
                return false;
            }
            if (this._randomA != other._randomA)
            {
                return false;
            }
            if (this._randomB != other._randomB)
            {
                return false;
            }
            if (this._randomC != other._randomC)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region operators
        public static bool operator ==(Ulid a, Ulid b)
        {
            if (a._timeHigh != b._timeHigh)
            {
                return false;
            }
            if (a._timeLow != b._timeLow)
            {
                return false;
            }
            if (a._randomA != b._randomA)
            {
                return false;
            }
            if (a._randomB != b._randomB)
            {
                return false;
            }
            if (a._randomC != b._randomC)
            {
                return false;
            }

            return true;
        }

        public static bool operator !=(Ulid a, Ulid b)
        {
            return !(a == b);
        }
        #endregion
    }
}
