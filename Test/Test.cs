using System;
using Memory.NET;

namespace Test
{
    class Test
    {
        private const long OffGlobalObjectManager = 0x10C90A0;
        private const long OffObjectManagerEntities = 0x22078;
        private const long NumEntities = 4096;

        static void Main(string[] args)
        {
            var mem = new MemoryAccess(22364);
            var objectManager = mem.Read<long>(mem.Base, OffGlobalObjectManager);
            for (var i = 0; i < NumEntities; i++)
            {
                var entity = mem.Read<long>(objectManager, OffObjectManagerEntities + (i * 8));
                Console.WriteLine(entity.ToString("X"));
            }

            Console.ReadKey();
        }
    }
}
