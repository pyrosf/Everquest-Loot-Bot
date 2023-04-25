using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using AutoHotkey.Interop;

namespace Everquest_Loot_Bot
{
    class Program
    {
        public class AuctionWinner
        {
            public string PlayerName;
            public int DKPAmount;
        }

        static void Main(string[] args)
        {
            
            
        // Load DKP file
        Dictionary<string, string> DKPList = Load_DKP.LoadDKP(Globals.OutputPath + "dkp.csv");
        Dictionary<string, string> TierList = Load_DKP.LoadTier(Globals.OutputPath + "dkp.csv");
            StreamWriter Writer = new StreamWriter(Globals.OutputPath + "Output.csv",true);
            //Writer.WriteLine("Name,Item,DKP");
            // Load Log file
            string FileName = "eqlog_Glasya_aradune.txt";
            string line = "";
            int ReadLineCount = 0;
            var ahk = AutoHotkeyEngine.Instance;
            ahk.ExecRaw("SetKeyDelay, 2");
            char DelChar = ' ';
            bool AuctionMode = false;
            String ItemForAuction = "";
            int CurrentWinningBid = 0;
            bool EndOfSetup = false;
            int PsudoTimer = 0;
            string bidder = "";
            int BiddingTier = 0;
            string Channel = "/ooc";
            



            while (EndOfSetup == false)
            {
                    StreamReader reader = new StreamReader(Globals.TargetPath + FileName);
                
                    int Lines = 0;
                    bool EOF = false;
                    while (Lines != ReadLineCount || EOF == true)
                    {
                        reader.ReadLine();
                        Lines++;
                    }
                    EOF = true;
                    line = reader.ReadLine();
                    while (line != null)
                    {
                        Console.WriteLine(line);
                        ReadLineCount++;
                        line = reader.ReadLine();

                    }
                reader.Close();
                reader.Dispose();
                EndOfSetup = true;
                // this allows me to ignore old text in the log
            }
            
            // Start main loop here

            while(true)
            {
                StreamReader reader = new StreamReader(Globals.TargetPath + FileName);

                int Lines = 0;
                bool EOF = false;
                while (Lines != ReadLineCount || EOF == true)
                {
                    reader.ReadLine();
                    Lines++;
                }
                EOF = true;








                if (AuctionMode == true)
                {
                    PsudoTimer--;
                    if (PsudoTimer % 5 == 0)
                    {

                        ahk.ExecRaw("SendEvent," + Channel + " " + PsudoTimer.ToString() + " Seconds left to bid on " + ItemForAuction);
                        Thread.Sleep(200);
                        ahk.ExecRaw("SendEvent,{Enter}");
                    }
                    
                    
                        
                    
                    if (PsudoTimer == 0)
                    {
                        if (CurrentWinningBid == 0)
                        {
                            ahk.ExecRaw("SendEvent,  " + Channel + " No One Wins " + ItemForAuction);
                            Thread.Sleep(200);
                            ahk.ExecRaw("SendEvent,{Enter}");
                            Writer.WriteLine(ItemForAuction + ",No One," + CurrentWinningBid);
                            Writer.Flush();
                        }
                        else
                        {
                            AuctionWinner Winner = new AuctionWinner();
                            Winner.PlayerName = bidder;
                            Winner.DKPAmount = CurrentWinningBid;
                            ahk.ExecRaw("SendEvent,  " + Channel + " Congrads " + bidder + " You have Won " + ItemForAuction + " for " + CurrentWinningBid.ToString() + " DKP {$}{$} GRATS `%`%");
                            Thread.Sleep(200);
                            ahk.ExecRaw("SendEvent,{Enter}");
                            Writer.WriteLine(ItemForAuction + "," + bidder + "," + CurrentWinningBid);
                            Writer.Flush();
                        }
                            AuctionMode = false;
                            bidder = "";
                            CurrentWinningBid = 0;
                            ItemForAuction = "";
                        
                        
                    }

                }
                
                line = reader.ReadLine();
                //[Tue May 26 12:40:01 2020] Eadanea tells the raid, 'bid 5'
                while (line != null)
                {
                        
                    {
                        string templine = line.Remove(0, 27);
                        string[] linesegments = templine.Split(DelChar);
                        linesegments[5] = linesegments[5].Remove(linesegments[5].Length - 1);
                        if (linesegments[5].All(Char.IsDigit))
                        {
                            if (!DKPList.ContainsKey(linesegments[0]))
                            {
                                ahk.ExecRaw("SendEvent," + Channel + " Sorry but "+ linesegments[0] + " Has no DKP");
                                Thread.Sleep(200);
                                ahk.ExecRaw("SendEvent,{Enter}");
                                continue;
                            }
                            if (int.Parse(linesegments[5]) > CurrentWinningBid && int.Parse(linesegments[5]) - CurrentWinningBid > 5)
                            {
                                int currentAmount = int.Parse(DKPList[linesegments[0]]);
                                if (currentAmount >= int.Parse(linesegments[5]))
                                {
                                    ahk.ExecRaw("SendEvent," + Channel + " Current bid for " + ItemForAuction + " {<}{!}{>} " + linesegments[0] + " at " + linesegments[4] + " DKP {$}{$}");
                                    Thread.Sleep(200);
                                    ahk.ExecRaw("SendEvent,{Enter}");
                                    CurrentWinningBid = int.Parse(linesegments[4]);
                                    bidder = linesegments[0];
                                    PsudoTimer = PsudoTimer + 5;
                                }
                                else
                                {
                                    ahk.ExecRaw("SendEvent," + Channel + " Sorry but " + linesegments[0] + " Only has " + DKPList[linesegments[0]] + " DKP");
                                    Thread.Sleep(200);
                                    ahk.ExecRaw("SendEvent,{Enter}");
                                }
                            }
                            else
                            {
                                if (int.Parse(linesegments[5]) - CurrentWinningBid < 5)
                                {
                                    ahk.ExecRaw("SendEvent," + Channel + " Sorry but " + linesegments[0] + " Did not bid enough");
                                    Thread.Sleep(200);
                                    ahk.ExecRaw("SendEvent,{Enter}");
                                    continue;
                                }
                            }
                        }

                    }
                    //Code here for stuff that happens when new text is found
                    //
                    //
                    //
                    //ahk.ExecRaw("SendEvent ,{Enter}");
                    //Thread.Sleep(500);
                    //ahk.ExecRaw("SendEvent , My First Script");
                    //Thread.Sleep(500);
                    //ahk.ExecRaw("SendEvent ,{Enter}");
                    //Thread.Sleep(500);

                    // this section reports the person who requeted their DKP (name is case sensetive)
                    if (line.ToLower().Contains("!my dkp"))
                    {
                        string templine = "";
                        //[Tue May 05 22:31:42 2020] Icrit tells you, '!DKP'
                        templine = line.Remove(0, 27);
                        string[] linesegments = templine.Split(DelChar);
                        Console.WriteLine(templine);
                        if (DKPList.ContainsKey(linesegments[0]))
                        {
                            
                            ahk.ExecRaw("SendEvent,/tell " + linesegments[0] + " Your DKP is:" + DKPList[linesegments[0]]);
                            Thread.Sleep(200);
                            ahk.ExecRaw("SendEvent,{Enter}");
                            Thread.Sleep(100);
                            Console.WriteLine("tell back successful W/ DKP");
                        }
                        else
                        {
                            
                            ahk.ExecRaw("SendEvent,/tell " + linesegments[0] + " You have no DKP");
                            Thread.Sleep(200);
                            ahk.ExecRaw("SendEvent,{Enter}");
                            Thread.Sleep(100);
                            Console.WriteLine("tell back successful");
                        }

                    }
                    if (line.ToLower().Contains("!request banded armor"))
                    {
                        string templine = "";

                        templine = line.Remove(0, 27);
                        string[] linesegments = templine.Split(DelChar);
                        ahk.ExecRaw("SendEvent,/tell " + linesegments[0] +" You have been put on the list for Banded Armor");
                        Thread.Sleep(200);
                        ahk.ExecRaw("SendEvent,{Enter}");
                        Thread.Sleep(100);
                        Console.WriteLine("Banded Armor Request Successful");
                        StreamWriter BandedArmorOutput = new StreamWriter(Globals.OutputPath + "BandedArmor.csv", true);
                        BandedArmorOutput.WriteLine(linesegments[0]);
                        BandedArmorOutput.Flush();
                        BandedArmorOutput.Close();

                    }
                    if (line.ToLower().Contains("!request adderall"))
                    {
                        string templine = "";

                        templine = line.Remove(0, 27);
                        string[] linesegments = templine.Split(DelChar);
                        ahk.ExecRaw("SendEvent,/tell " + linesegments[0] + " I am not a doctor");
                        Thread.Sleep(200);
                        ahk.ExecRaw("SendEvent,{Enter}");
                        Thread.Sleep(100);
                        Console.WriteLine("No Adderall for you");
                        

                    }
                    //[Tue May 05 22:31:42 2020] Icrit tells you, '!Request Spell (Spell Name)'
                    if (line.ToLower().Contains("!request spell"))
                    {
                        string templine = "";

                        templine = line.Remove(0, 27);
                        string[] linesegments = templine.Split(DelChar);
                        string JoinedArray = string.Join(" ", linesegments.Skip(5));
                        JoinedArray = JoinedArray.Remove(JoinedArray.Length - 1);
                        ahk.ExecRaw("SendEvent,/tell " + linesegments[0] + " You have requested " + JoinedArray);
                        Thread.Sleep(200);
                        ahk.ExecRaw("SendEvent,{Enter}");
                        Thread.Sleep(100);
                        Console.WriteLine("Spell Request Successful");
                        StreamWriter BandedArmorOutput = new StreamWriter(Globals.OutputPath + "SpellRequest.csv", true);
                        BandedArmorOutput.WriteLine(linesegments[0]+","+JoinedArray);
                        BandedArmorOutput.Flush();
                        BandedArmorOutput.Close();
                    }



                        //[Tue May 05 22:31:42 2020] Icrit tells you, '!Report DKP (PlayerName)'
                        // Report other players DKP
                        if (line.ToLower().Contains("!report dkp"))
                    {
                        string templine = "";

                        templine = line.Remove(0, 27);
                        string[] linesegments = templine.Split(DelChar);
                        //str.Remove(str.Length - 1);
                        linesegments[5] = linesegments[5].Remove(linesegments[5].Length - 1);
                        Console.WriteLine(templine);

                        if (DKPList.ContainsKey(linesegments[5]))
                        {
                            
                            ahk.ExecRaw("SendEvent,/tell " + linesegments[0] + " The Player " + linesegments[5] + " Has:" + DKPList[linesegments[5]] + " DKP");
                            Thread.Sleep(200);
                            ahk.ExecRaw("SendEvent,{Enter}");
                            Thread.Sleep(100);
                            Console.WriteLine("tell back successful W/ DKP");
                        }
                        else
                        {
                            
                            ahk.ExecRaw("SendEvent,/tell " + linesegments[0] + " The Player " + linesegments[5] + " Has No DKP");
                            Thread.Sleep(200);
                            ahk.ExecRaw("SendEvent,{Enter}");
                            Thread.Sleep(100);
                            Console.WriteLine("tell back successful");
                        }

                    }
                    //[Tue May 05 22:31:42 2020] Icrit tells you, '!Bid (Item Name)'
                    if (line.Contains("!Auction") && AuctionMode == false && !line.Contains("You say"))
                    {
                        string templine = "";

                        templine = line.Remove(0, 27);
                        string[] linesegments = templine.Split(DelChar);
                        string JoinedArray = string.Join(" ", linesegments.Skip(4));
                        JoinedArray = JoinedArray.Remove(JoinedArray.Length - 1);
                        ItemForAuction = JoinedArray;
                        ahk.ExecRaw("SendEvent," + Channel + " Bidding has started for [" + ItemForAuction + "] {<}{!}{>} Only Classes that can use this item may bid {#}{#}");
                        Thread.Sleep(200);
                        ahk.ExecRaw("SendEvent,{Enter}");
                        AuctionMode = true;
                        
                        PsudoTimer = 30;

                    }

                    // Bot can start the auction, but far better if it does not bid
                    if (line.Contains("!Auction") && AuctionMode == false && line.Contains("You say"))
                    {
                        string templine = "";

                        templine = line.Remove(0, 27);
                        string[] linesegments = templine.Split(DelChar);
                        string JoinedArray = string.Join(" ", linesegments.Skip(3));
                        JoinedArray = JoinedArray.Remove(JoinedArray.Length - 1);
                        ItemForAuction = JoinedArray;
                        ahk.ExecRaw("SendEvent," + Channel + " Bidding has started for [" + ItemForAuction + "] {<}{!}{>} Only Classes that can use this item may bid {#}{#}");
                        Thread.Sleep(200);
                        ahk.ExecRaw("SendEvent,{Enter}");
                        AuctionMode = true;

                        PsudoTimer = 30;

                    }




                    //[Mon May 18 09:39:17 2020]
                    //You say, '!Auction BBC'


                    Console.WriteLine(line);
                    ReadLineCount++;
                    line = reader.ReadLine();

                }
                reader.Close();
                reader.Dispose();
                
                Thread.Sleep(1000);// Prevent script from breaking file read



            }



        }
    }
}

