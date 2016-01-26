using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace TplSample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var generatorBlock = new TransformManyBlock<int, string>(num => GenerateStrings(num));
            var writerBlock = new TransformBlock<string, string>(str => WriteString(str), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });
            var finishingBlock = new ActionBlock<string>(str =>
            {
                writerBlock.Completion.Wait();
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ": finished - " + str);
            });

            generatorBlock.LinkTo(writerBlock, new DataflowLinkOptions{PropagateCompletion = true});
            writerBlock.LinkTo(finishingBlock, new DataflowLinkOptions { PropagateCompletion = true });

            for (var i = 1; i <= 3; i++)
            {
                Console.WriteLine("Posted " + i*10);
                generatorBlock.Post(i*10);
            }
            generatorBlock.Complete();
            finishingBlock.Completion.Wait();

            Console.WriteLine("Pipeline is finished");
            Console.ReadKey();
        }

        private static void FinishingAction(string str)
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ": finished - " + str);
        }

        private static string WriteString(string str)
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ": " + str);
            Thread.Sleep(1000);
            return str;
        }

        private static string[] GenerateStrings(int num)
        {
            var result = new List<string>();
            var str = string.Empty;
            for (var i = num; i <= num + 5; i++)
            {
                str += "." + i;
                result.Add(str);
            }
            return result.ToArray();
        }
    }
}