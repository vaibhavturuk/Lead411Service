Usage
===============================================================================


Automated Senders
-------------------------------------------------------------------------------
using Dot.MktData;
using DotCommon.Utils.Mail.TransportHeaders;
using DotIFaces.Interfaces.Mail;
using DotIFaces.Interfaces.MktData;
using Ninject;
using System;

namespace TestNuget
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var k = new StandardKernel(new MktDataModule()))
            {
                var asp = k.Get<AutomatedSenderProvider>();

                var amIARobot = asp.IsMatch("noreply@Lead411.com");
                var output = amIARobot ? "bleep boop" : "negative";
                Console.WriteLine($@"Am I a robot? {amIARobot}");
            }

        }
    }
}
