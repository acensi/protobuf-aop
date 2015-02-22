namespace ProtoBufAOP
{
    [AutoProtoContract]
    public class BaseA
    {
        public int Test
        {
            get;
            set;
        }

        private int _untouched;

        public string ReadOnly
        {
            get
            {
                return "Read-Only string";
            }
        }

        public float OtherProperty
        {
            get;
            private set;
        }
    }

    public class DerivedFromA : BaseA
    {
        public string DerivedString
        {
            get;
            set;
        }
    }

    public class DerivedFromDerivedFromA : DerivedFromA
    {
        public string DerivedFromDerivedString
        {
            get;
            set;
        }
    }

    public class DerivedFromA1 : BaseA
    {
        public string ReadOnlyA1
        {
            get;
            set;
        }
    }

    public class BaseB
    {

    }

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
