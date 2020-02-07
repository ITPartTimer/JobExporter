using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JOBExporter
{
    #region Expand Stratix Arbor
    // Get list of start Pos for each setup
    List<int> lstStartPos = new List<int>();

            foreach (Consume c in lstConsume)
            {
                int p = 1;

                // Start of 1st setup is position 1 on arbor
                if (c.Pos != 1)
                    p = c.Pos;

                lstStartPos.Add(p);
            }

// Expand Arbor from Stratix arbor
List<ArborStratix> lstExp = new List<ArborStratix>();

int aPos = 0;
int aSetupCount = 0;

// First setup Pos=1, but 1st object in List<> = 0
int aSetup = lstStartPos[aSetupCount];

            foreach (ArborStratix a in lstArborStratix)
            {
                //Exp Nbr into Nbr object in lstExp
                for (int i = 1; i <= a.Nbr; i++)
                {
                    // Start Pos counter with 1, then ++ for each pass
                    aPos++;

                    // Look ahead to start Pos of next setup
                    if (aSetupCount <= lstStartPos.Count)
                    {
                        // If current Pos = start of next setup, move to next setup
                        if (lstStartPos[aSetupCount + 1] == aPos)
                            aSetup++;
                    }

                    ArborStratix aExp = new ArborStratix();

aExp.Job = string.Concat(job.ToString(), "-", aSetup.ToString());
aExp.Wdth = a.Wdth;
                    //aExp.Pos = aPos;
                    aExp.Nbr = 1; //Expanded each cut has a qty = 1

                    lstExp.Add(aExp);
                }
            }

            Console.WriteLine("==== ARBOR EXAPNDED ====");
            foreach (ArborStratix f in lstExp)
            {
                Console.WriteLine(f.Job + " / " + f.Wdth.ToString() + " / " + f.Nbr.ToString());
            }

            var lstArborCol = lstExp.GroupBy(x => x.Job);

List<ArborStratix> lstArborSetups = new List<ArborStratix>();

Console.WriteLine("==== ARBOR COLLAPSED INTO SETUPS ====");
            foreach (var sGroup in lstArborCol)
            {
                foreach (ArborStratix a in sGroup)
                {
                    ArborStratix arb = new ArborStratix();

arb.Job = a.Job;
                    arb.Wdth = a.Wdth;
                    arb.Nbr = a.Nbr;

                    lstArborSetups.Add(arb);
                }
            }

            Console.WriteLine(" ==== SETUPS: " + lstArborSetups.Count().ToString() + " ====");
            foreach (ArborStratix a in lstArborSetups)
            {
                Console.WriteLine(a.Job + " / " + a.Wdth.ToString() + " / " + a.Nbr.ToString());
            }

            #endregion

            /*
            Build List<MultFile>
            */

            List<MultFile> lstMultFile = new List<MultFile>();

            if (lstConsume.Count == 1)
            {
                // Only one setup
                foreach (ArborStratix a in lstArborStratix)
                {
                    MultFile m = new MultFile();

m.Job = lstNumSetups[0];
                    m.Qty = a.Nbr;
                    m.Size = a.Wdth;

                    // Find matchind Wdth in lstDetail. 
                    // There is NO link between Wdth in ArborStratix and Wdth on SO
                    var detail = lstDetail.Where(x => x.Wdth == a.Wdth).Distinct();

                    foreach(MultDetail d in detail)
                    {
                        // Should only be one object
                        m.WdthP = d.WdthP;
                        m.WdthN = d.WdthN;
                    }                  

                    lstMultFile.Add(m);
                }
            }
            else
            {
                // More than one setup

                //// Get list of start Pos for each setup
                //List<int> lstStartPos = new List<int>();

                //foreach (Consume c in lstConsume)
                //{
                //    int p = 1;

                //    // Start of 1st setup is position 1 on arbor
                //    if (c.Pos != 1)
                //        p = c.Pos;

                //    lstStartPos.Add(p);
                //}

                //// Expand Arbor from Stratix arbor
                //List<ArborExp> lstExp = new List<ArborExp>();

                //int aPos = 0;
                //int aSetupCount = 0;

                //// First setup Pos=1, but 1st object in List<> = 0
                //int aSetup = lstStartPos[aSetupCount];

                //foreach(ArborStratix a in lstArborStratix)
                //{   
                //    //Exp Nbr into Nbr object in lstExp
                //    for (int i = 1; i <= a.Nbr; i++)
                //    {
                //        // Start Pos counter with 1, then ++ for each pass
                //        aPos++;

                //        // Look ahead to start Pos of next setup
                //        if (aSetupCount <= lstStartPos.Count)
                //        {
                //            // If current Pos = start of next setup, move to next setup
                //            if (lstStartPos[aSetupCount + 1] == aPos)
                //                aSetup++;
                //        }

                //        ArborExp aExp = new ArborExp();

                //        aExp.Job = string.Concat(job.ToString(),"-",aSetup.ToString());
                //        aExp.SetUp = aSetup;
                //        aExp.Wdth = a.Wdth;
                //        aExp.Pos = aPos;

                //        lstExp.Add(aExp);
                //    }
                //}

                //// test
                //Console.WriteLine("==== ARBOR EXP ====");
                //foreach (ArborExp f in lstExp)
                //{
                //    Console.WriteLine(f.Job + " / " + f.SetUp.ToString() + " / " + f.Wdth.ToString() + " / " + f.Pos.ToString());
                //}

                //// Break ArborFull into separate setups by grouping on ArborFull.Setup
                //var lstSetupsAll = lstExp.GroupBy(x => x.SetUp);

                //// NOTE: NEED TO GROUP BY WDTH AND PUT IN NEW LIST

                ///*
                //Build MultFile

                //lstSetupsAll is an iGrouping<>
                //Need to nest  the object foreach() inside a var foreach()
                //*/
                //foreach(var sGroup in lstSetupsAll)
                //{
                //    foreach (ArborExp a in sGroup)
                //    {
                //        MultFile m = new MultFile();

                //        m.Job = a.Job;
                //        m.Qty = 1; // NEED TO FIX
                //        m.Size = a.Wdth;

                //        var detail = lstDetail.Where(x => x.Wdth == a.Wdth).Distinct();

                //        foreach (MultDetail d in detail)
                //        {
                //            // Should only be one object
                //            m.WdthP = d.WdthP;
                //            m.WdthN = d.WdthN;
                //        }

                //        lstMultFile.Add(m);
                //    }
                    
                //}
            }

            //testing
            Console.WriteLine("==== MULTFILE ====");
            foreach (MultFile m in lstMultFile)
            {
                Console.WriteLine(m.Job + " / " + m.Cust + " / " + m.Qty.ToString() + " / " + m.Size.ToString() + " / " + m.WdthP.ToString() + " / " + m.WdthN.ToString() + " / " + m.Knife.ToString());
            }
}

    // Add last Pos on arbor.  +1, since stopPos = -1
            lstStartPos.Add(lstArborStratix.Sum(x => x.Nbr) + 1);

            List<ArborStratix> lstMults = new List<ArborStratix>();

int accPos = 0;
int setupCnt = 1;
int partialNbr = 0;
int balNbr = 0;
int posStratixArbor = 0;

            foreach (int p in lstStartPos)
            {   
               
                int stopPos = p - 1;

                // Step thru each member of lstArborStratix
                for (int i = posStratixArbor; i<lstArborStratix.Count(); i++)
                {
                    int accNbr = lstArborStratix[i].Nbr + accPos;

                    if (accNbr <= stopPos)
                    {
                        // Update the Job to represent the setup -1,-2, and so on
                        lstArborStratix[i].Job = string.Concat(job.ToString(), "-", setupCnt.ToString());
accPos = accPos + lstArborStratix[i].Nbr;
                    }
                    else
                    {
                        lstArborStratix[i].Job = string.Concat(job.ToString(), "-", setupCnt.ToString());

// Figure how many Nbr your need from lstMult[i].Nbr and the balance
partialNbr = lstArborStratix[i].Nbr - (stopPos - accPos);
                        balNbr = lstArborStratix[i].Nbr - partialNbr;

                        // If there is a partial, update current member and add member with balance
                        if (partialNbr != lstArborStratix[i].Nbr)
                        {
                            // Update Nbr of just added member to what you need to complete this setup
                            lstArborStratix[i].Nbr = partialNbr;

                            // Move to new setup
                            setupCnt++;

                            // Create a new member for the List that contains the balance of Nbr
                            // This is on a new setup
                            ArborStratix a = new ArborStratix();

a.Job = string.Concat(job.ToString(), "-", setupCnt.ToString());
a.Wdth = lstArborStratix[i].Wdth;
                            a.Nbr = balNbr;

                            // All new member to List right after current index
                            lstArborStratix.Insert((i + 1), a);
                        }
                        
                        posStratixArbor = 1 + 1; // Might need to be +2
                        i = lstArborStratix.Count();
                    }
                }              
            }

            Console.WriteLine("==== UPDATED ARBOR ====");
            foreach (ArborStratix f in lstArborStratix)
            {
                Console.WriteLine(f.Job + " / " + f.Wdth.ToString() + " / " + f.Nbr.ToString());
            }
