using System;
using SkipList;

namespace ConsoleTest
{
    public class CT
    {
        static void Main()
        {
            SkipList<String, String> sl = new SkipList<String, String>();
            Console.WriteLine("Adding foo as fie.");
            sl.add("foo", "fie");
            Console.WriteLine("Is foo fie?");
            if ("fie".CompareTo(sl.get("foo")) == 0)
                Console.WriteLine("  Yes!");
            else
                Console.WriteLine("  No! :(");
        }
    }
}
