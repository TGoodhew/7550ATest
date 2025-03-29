using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17550Test
{
    class Program
    {
        public static GpibSession gpibSession;
        public static NationalInstruments.Visa.ResourceManager resManager;
        public static int gpibIntAddress = 6;

        static void Main(string[] args)
        {
            int X1 = 0, Y1 = 0, X2 = 0, Y2 = 0, X3 = 0, Y3 = 0, X4 = 0, Y4 = 0;

            // Setup the GPIB connection via the ResourceManager
            resManager = new NationalInstruments.Visa.ResourceManager();

            // Create a GPIB session for the specified address
            gpibSession = (GpibSession)resManager.Open(string.Format("GPIB0::{0}::INSTR", gpibIntAddress));
            gpibSession.TimeoutMilliseconds = 2000; // Set the timeout to be 2s
            gpibSession.TerminationCharacterEnabled = true;
            gpibSession.Clear(); // Clear the session

            Console.WriteLine("Ensure paper is loaded");

            // For some reason the GPIB communication seems to get stuck every now and then on this
            // I'm not sure if it is a problem with my setup (NI 2025 Q1 libraries & NI GPIB-ENET/1000) or the 7550A
            // but I have added a try/catch to handle the timeout exception that occurs
            //
            // TODO: investigate above issue and remove the try/catch
            try
            {
                // Setup buffer
                gpibSession.FormattedIO.WriteLine((char)27 + ".T1000;6000;0;0;5800:");
                // Confirm the IO Buffer
                gpibSession.FormattedIO.WriteLine((char)27 + ".L");
                Console.WriteLine("The IO Buffer is set to " + gpibSession.FormattedIO.ReadString() + " bytes");

                // PG IN OP
                gpibSession.FormattedIO.WriteLine("PG;IN;OP;");
                var result = gpibSession.FormattedIO.ReadString();

                string[] values = result.Split(',');

                X1 = int.Parse(values[0]);
                Y1 = int.Parse(values[1]);
                X2 = int.Parse(values[2]);
                Y2 = int.Parse(values[3]);

                Console.WriteLine($"X1: {X1}, Y1: {Y1}, X2: {X2}, Y2: {Y2}");

                // OW
                gpibSession.FormattedIO.WriteLine("OW;");
                result = gpibSession.FormattedIO.ReadString();

                values = result.Split(',');

                X3 = int.Parse(values[0]);
                Y3 = int.Parse(values[1]);
                X4 = int.Parse(values[2]);
                Y4 = int.Parse(values[3]);

                Console.WriteLine($"X3: {X3}, Y3: {Y3}, X4: {X4}, Y4: {Y4}");

                // Reset the timeout to 40s
                gpibSession.TimeoutMilliseconds = 40000;
            }
            catch (Ivi.Visa.IOTimeoutException ex)
            {
                Console.WriteLine("Buffer read failed - {0}", ex.Message);
                Environment.Exit(1);
            }

            // DRAW+ AT P1 & P2 & LABEL COORDINATES
            gpibSession.FormattedIO.WriteLine("SP1;PA5100,4064;PD;SM+;PU" + X1 + "," + Y1);
            gpibSession.FormattedIO.WriteLine("CP2,-.3;LBP1=(" + X1 + "," + Y1 + ")" + (char)3);
            gpibSession.FormattedIO.WriteLine("PA" + X2 + "," + Y2 + ";SM;");
            gpibSession.FormattedIO.WriteLine("CP-16,-.3;LBP2=(" + X2 + "," + Y2 + ")" + (char)3);

            gpibSession.FormattedIO.WriteLine("PA2032,6236;");
            PenRepeatabilityType1(1); // This routine is called multiple times and creates a cross that shows the pens hitting the same points
            gpibSession.FormattedIO.WriteLine("PA8128,1892;");
            PenRepeatabilityType2(1); // Same idea as the previous one but a slightly different process

            gpibSession.FormattedIO.WriteLine("FT4,100,45;PA9372,6440;RR700,700;SP2;ER700,700");

            // DRAW & LABEL AXIS
            gpibSession.FormattedIO.WriteLine("PA9124,1016;PD;");
            for (int i = 0; i < 8; i++)
            {
                gpibSession.FormattedIO.WriteLine("XT;PR-1016,0;");
            }

            for (int i = 0; i < 15; i++)
            {
                gpibSession.FormattedIO.WriteLine("PR0,400;YT;");
            }

            gpibSession.FormattedIO.WriteLine("PU;PA2032,4788;");
            PenRepeatabilityType1(2);
            gpibSession.FormattedIO.WriteLine("PA8128,3340;");
            PenRepeatabilityType2(2);

            gpibSession.FormattedIO.WriteLine("FT4,50,90;PA9722,5600;WG350,0,360,40;SP3;EW350,0,360,40;");
            gpibSession.FormattedIO.WriteLine("SP3;PA600,3500;DI0,1;LBCentimetres" + (char)3 + ";");
            gpibSession.FormattedIO.WriteLine("PA700,6966;DI;");

            for (int i = 15; i > 0; i--)
            {
                if (i < 10)
                {
                    gpibSession.FormattedIO.WriteLine("CP1,0;");
                }

                gpibSession.FormattedIO.WriteLine("LB" + i + (char)13 + (char)3 + ";PR0,-400;");
            }

            gpibSession.FormattedIO.WriteLine("PA2032,3340;");
            PenRepeatabilityType1(3);
            gpibSession.FormattedIO.WriteLine("PA8128,4788;");
            PenRepeatabilityType2(3);

            gpibSession.FormattedIO.WriteLine("UF10,5,5;FT5;PA9722,4060;PT.5;WG700,60,60;");
            gpibSession.FormattedIO.WriteLine("PA948,756;SP4;");

            for (int i = 0; i < 8; i++)
            {
                gpibSession.FormattedIO.WriteLine("LB" + i + (char)13 + (char)3 + ";PR1016,0;");
            }

            gpibSession.FormattedIO.WriteLine("PA4810,516;LBInches" + (char)3);
            gpibSession.FormattedIO.WriteLine("PA2032,1892;");
            PenRepeatabilityType1(4);
            gpibSession.FormattedIO.WriteLine("PA8128,6236;");
            PenRepeatabilityType2(4);

            gpibSession.FormattedIO.WriteLine("UF12,8;FT5;PA9722,3570;PT.5;WG700,240,60;SP5;EW700,240,60;");
            gpibSession.FormattedIO.WriteLine("PU8128,6236;");
            PenRepeatabilityType1(5);
            gpibSession.FormattedIO.WriteLine("PA2032,1892;");
            PenRepeatabilityType2(5);

            // DRAW CIRCULAR FAN

            gpibSession.FormattedIO.WriteLine("PA5100,4064;PM0;");

            for (int i = 108; i <= 608; i += 100)
            {
                gpibSession.FormattedIO.WriteLine("CI" + i + ";PM1;");
            }

            gpibSession.FormattedIO.WriteLine("PM2;UF;FT5;FP;SP6;EP;SP7;");
            gpibSession.FormattedIO.WriteLine("PA8128,3340;");
            PenRepeatabilityType1(7);
            gpibSession.FormattedIO.WriteLine("PA2031,4788;");
            PenRepeatabilityType2(7);

            gpibSession.FormattedIO.WriteLine("IW3600,2564,6600,5564;PA3600,2564;ER3000,3000;SP8;");

            // Convert degrees to radians for trigonometric functions
            double degreesToRadians = Math.PI / 180.0;

            for (int i = 0; i < 360; i += 15)
            {
                double radians = i * degreesToRadians;

                // Calculate the edge of the circle
                int circleStartLineX = (int)Math.Round(5100 + 608 * Math.Cos(radians));
                int circleStartLineY = (int)Math.Round(4064 + 608 * Math.Sin(radians));
                int rectEndLineX = (int)Math.Round(5100 + 2200 * Math.Cos(radians));
                int rectY = (int)Math.Round(4064 + 2200 * Math.Sin(radians));

                Console.WriteLine($"Circle Edge: ({circleStartLineX}, {circleStartLineY}), Rectangle Edge: ({rectEndLineX}, {rectY})");
                gpibSession.FormattedIO.WriteLine("PU" + circleStartLineX + "," + circleStartLineY + ";PD" + rectEndLineX + "," + rectY + ";");
            }

            gpibSession.FormattedIO.WriteLine("IW;PU8128,1892;");
            PenRepeatabilityType1(8);
            gpibSession.FormattedIO.WriteLine("PA2032,6236;");
            PenRepeatabilityType2(8);

            // DRAW LABELS

            gpibSession.FormattedIO.WriteLine("PA3610,6514;");
            gpibSession.FormattedIO.WriteLine("VS;SI1,1;SL.45;LB7550A" + (char)3 + ";");
            gpibSession.FormattedIO.WriteLine("PA4645,1778;");
            gpibSession.FormattedIO.WriteLine("SI;SL;LBFeatures" + (char)3 + ";");
            gpibSession.FormattedIO.WriteLine("CP-6,-1;LBPlot" + (char)3 + ";");
            gpibSession.FormattedIO.WriteLine("PA8128,4788;");
            PenRepeatabilityType1(6);
            gpibSession.FormattedIO.WriteLine("PA2032,3340;");
            PenRepeatabilityType2(6);
            gpibSession.FormattedIO.WriteLine("FT4,100,45;PA9372,490;RR700,700;SP1;ER700,700");

            // FRAME WINDOW

            gpibSession.FormattedIO.WriteLine("PU" + X3 + "," + Y3 + ";EA" + X4 + "," + Y4 + ";");
            gpibSession.FormattedIO.WriteLine("PU5100,4064;CI25;SP0;PA" + X4 + "," + Y4 + ";");
        }

        // PEN TO PEN REPEATABILITY
        private static void PenRepeatabilityType2(int pass)
        {
            gpibSession.FormattedIO.WriteLine("CP.4,-.8;LB" + pass + (char)3 + ";CP-1.4,.8;");
            gpibSession.FormattedIO.WriteLine("PR0,512;PD0,-1024;PU-512,512;PD1024,0;PU;");
        }

        // PEN TO PEN REPEATABILITY
        static void PenRepeatabilityType1(int pass)
        {
            gpibSession.FormattedIO.WriteLine("SI;CP-1.2,.4;LB" + pass + (char)3 + "; CP.2,-.4;");
            gpibSession.FormattedIO.WriteLine("PR9,-9;PD247,0,0,18,-247,0,0,247,-18,0,0,-247,-247,0,0,-18,247,0,0,-247,18,0,0,247;PU;");
        }
    }
}
