using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JOBExporter.Models;
using JOBExporter.DAL;
using JOBExporter.XLS;

namespace JOBExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Args
            List<string> sArgs = new List<string>();

            string job = string.Empty;

            Console.WriteLine("Enter Job:");
            job = Console.ReadLine();

            //try
            //{
            //    if (args.Length > 0)
            //        job = args[0].ToString();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Arg ex: " + ex.Message);
            //}
            #endregion

            #region Consume
            /*
            Get Planned Consumption from Stratix.
            The number of rows is the number of setups on this Job
            Sort by Seq.  Use Pos information to determine where 
            at what arbor position to start each setup
            */
            DataAccess objConsume = new DataAccess();

            List<Consume> lstConsume = new List<Consume>();

            lstConsume = objConsume.Get_Consumed(job);

            // Testing Change Pos on 2nd element to 12
            //var editConsume = from c in lstConsume
            //              where c.Pos == 16
            //              select c;

            //foreach(Consume c in editConsume)
            //{
            //    c.Pos = 18;
            //}

            // test
            Console.WriteLine("==== CONSUME ====");
            foreach (Consume c in lstConsume)
            {
                Console.WriteLine(c.Job.ToString() + " / " + c.Seq.ToString() + " / " + c.Tag + " / " + c.Wdth.ToString() + " / " + c.Stp.ToString() + " / " + c.Pos.ToString());
            }
            #endregion           

            #region SO
            /*
            Get Unique SOs from Stratix.
            Planned Production iptjpp_rec will list each SO on the Job
            Parse this into a Prefix, Ref, Item and SubItem.
            This will be used to query Transaction Common Items joined with
            CPS Tolerances to get item details.           
            */
            DataAccess objSO = new DataAccess();

            List<SO> lstSO = new List<SO>();

            lstSO = objSO.Get_SOs(job);

            // test
            Console.WriteLine("==== SO ====");
            foreach (SO s in lstSO)
            {
                Console.WriteLine(s.Trgt + " / " + s.Pfx + " / " + s.Ref + " / " + s.Itm + " / " + s.SItm);
            }

            // Build a list of Ref + Item to be used in the query for Item Details
            string inQry = "";

            foreach (SO s in lstSO)
            {
                inQry = inQry + string.Concat(s.Ref, s.Itm, ",");
            }

            inQry = inQry.TrimEnd(',');

            // test
            //Console.WriteLine("inQry: " + inQry);
            #endregion

            #region MultDetail
            /*
            Transaction Common Item Product joined with CPS Tolerances will
            give you all the detail you need about the cut for each SO (ex:56572-54)
            on the Job
            */
            DataAccess objDetail = new DataAccess();

            List<MultDetail> lstDetail = new List<MultDetail>();

            lstDetail = objDetail.Get_Details(inQry);

            // test
            Console.WriteLine("==== MULT DETAIL ====");
            foreach (MultDetail m in lstDetail)
            {
                Console.WriteLine(m.Pfx + " / " + m.Ref + " / " + m.Itm + " / " + m.Cus + " / " + m.Part + " / " + m.CtlNo.ToString() + " / " + m.Frm + " / " + m.Ga.ToString() + " / " + m.GaP.ToString() + " / " + m.GaN.ToString() + " / " + m.Wdth.ToString() + " / " + m.WdthP.ToString() + " / " + m.WdthN.ToString());
            }
            #endregion

            #region Ga
            /*
            Get the Gauge listed on the Job
            
            Determine the most constrained gauge range from the CPS on the Job
            */

            // Consume query was ordered by seq, so first member contains the TagNo for the job
            string tag = lstConsume.Select(x => x.Tag).First().ToString();

            DataAccess objGa = new DataAccess();

            Ga g = new Ga();

            g = objGa.Get_Ga(tag, lstDetail);
            #endregion

            #region Build HdrFile          
            /*
            Build List<HdrFile>
            */

            // Determine number of setups
            int numSetups = lstConsume.Count;
            List<string> lstNumSetups = new List<string>();

            //Add sufix of 1,2,3... to end of Job
            if (numSetups == 1)
                lstNumSetups.Add(string.Concat(job.ToString(), "-", "1"));
            else
                for (int i = 0; i < numSetups; i++)
                    lstNumSetups.Add(string.Concat(job.ToString(), "-", (i + 1).ToString()));


            List<HdrFile> lstHdr = new List<HdrFile>();

            foreach (string j in lstNumSetups)
            {
                HdrFile h = new HdrFile();

                h.Job = j;
                h.Mtl = lstDetail[0].Frm;
                h.Wdth = lstConsume[0].Wdth;
                h.Ga = g.NumSize;
                h.Clr = h.KnifeClr * h.Ga;             
                h.GaP = g.GaP;
                h.GaN = g.GaN;              

                // After Pos = 1, if consecutive Pos are same value, you are reslitting
                //FIX THIS TO ADD RESLIT TO NOTE
                //if (lstConsume[j].Pos != 1)
                //{
                //    if (lstConsume[j].Pos == lstConsume[j - 1].Pos)
                //        h.Note = "RESLIT";
                //}

                lstHdr.Add(h);
            }

            // testing
            Console.WriteLine("==== HDR FILE ====");
            foreach (HdrFile h in lstHdr)
            {
                Console.WriteLine(h.Job + " / " + h.Cust + " / " + h.Mtl + " / " + h.Wdth.ToString() + " / " + h.Ga.ToString() + " / " + h.KnifeClr.ToString() + " / " + h.Clr.ToString() + " / " + h.GaP.ToString() + " / " + h.GaN.ToString() + " / " + h.Note);
            }
            #endregion

            #region Build MultFile
            /*
            Build MultFile
            */

            // Get Arbor from Stratix
            DataAccess objArbor = new DataAccess();

            List<ArborStratix> lstArborStratix = new List<ArborStratix>();

            lstArborStratix = objArbor.Get_Arbors(job);

            // test
            Console.WriteLine("==== ARBOR ====");
            foreach (ArborStratix a in lstArborStratix)
            {
                Console.WriteLine(a.Job.ToString() + " / " + a.Wdth.ToString() + " / " + a.Nbr.ToString());
            }

            // Get list of start Pos for each setup
            List<int> lstStartPos = new List<int>();

            foreach (Consume c in lstConsume)
                lstStartPos.Add(c.Pos);

            // Expand lstArborStratix to assign Job, setup and position
            List<ArborExp> lstExp = new List<ArborExp>();

            int aPos = 0; //Arbor position 1 to X
            int aSetupCount = 0;

            // Suffix on Job is current setup on Job
            // First setup Pos=1, but 1st object in List<> = 0
            int aSetup = lstStartPos[aSetupCount];

            foreach (ArborStratix a in lstArborStratix)
            {
                //Look at each cut in the setup and expand into setup with only single cuts
                for (int i = 1; i <= a.Nbr; i++)
                {
                    // Start position counter with 1, then ++ for each pass
                    aPos++;

                    // If there is only on setup start Pos
                    if (lstStartPos.Count == 1)
                        aSetup = 1;                  
                    else
                    {
                        // Look ahead to start Pos of next setup
                        if (aSetupCount <= lstStartPos.Count)
                        {
                            // If current position = start Pos of next setup, increment the setup count
                            if (lstStartPos[aSetupCount + 1] == aPos)
                                aSetup++;
                        }       
                    }

                    ArborExp aExp = new ArborExp();

                    aExp.Job = string.Concat(job.ToString(), "-", aSetup.ToString());
                    aExp.Wdth = a.Wdth;
                    aExp.Pos = aPos;

                    lstExp.Add(aExp);
                }
            }

            Console.WriteLine("==== ARBOR EXPANDED ====");
            foreach (ArborExp f in lstExp)
            {
                Console.WriteLine(f.Job + " / " + f.Wdth.ToString() + " / " + f.Pos.ToString());
            }


            int indexClp = 0;

            // Collapse Arbor counting number of repeat sizes as you go
            List<ArborStratix> lstArborClp = new List<ArborStratix>();

            for (int i = 0; i < lstExp.Count(); i++)
            {
                // 1st Position always gets added to the List
                if (lstExp[i].Pos == 1)
                {
                    ArborStratix a = new ArborStratix();

                    a.Job = lstExp[i].Job;
                    a.Wdth = lstExp[i].Wdth;
                    a.Nbr = 1; // Default to get the count started

                    lstArborClp.Add(a);
                }
                else
                {
                    // If Wdth of next element is same as previous
                    if (lstExp[i].Wdth == lstArborClp[indexClp].Wdth)
                    {
                        // lstExp.Wdth is same as previous lstArborClp, so just ++ Nbr
                        lstArborClp[indexClp].Nbr++;
                    }
                    else
                    {
                        // New wdth, so add to lstArborClp
                        ArborStratix a = new ArborStratix();

                        a.Job = lstExp[i].Job;
                        a.Wdth = lstExp[i].Wdth;
                        a.Nbr = 1; // Default to get the count started

                        lstArborClp.Add(a);

                        // Inc index of lstArborClp
                        indexClp++;
                    }
                }
            }

            Console.WriteLine("==== ARBOR COLLAPSED ====");
            foreach (ArborStratix f in lstArborClp)
            {
                Console.WriteLine(f.Job + " / " + f.Wdth.ToString() + " / " + f.Nbr.ToString());
            }

            // Build MultFile
            List<MultFile> lstMults = new List<MultFile>();

            foreach(ArborStratix a in lstArborClp)
            {
                MultFile m = new MultFile();

                m.Job = a.Job;
                m.Qty = a.Nbr;
                m.Size = a.Wdth;
                m.WdthP = Convert.ToDecimal(lstDetail.Where(x => x.Wdth == a.Wdth).Select(x => x.WdthP).FirstOrDefault());
                m.WdthN = Convert.ToDecimal(lstDetail.Where(x => x.Wdth == a.Wdth).Select(x => x.WdthN).FirstOrDefault());

                lstMults.Add(m);
            }

            // testing
            Console.WriteLine("==== MULT FILE ====");
            foreach (MultFile h in lstMults)
            {
                Console.WriteLine(h.Job + " / " + h.Cust + " / " + h.Qty.ToString() + " / " + h.Size.ToString() + " / " + h.WdthP.ToString() + " / " + h.WdthN.ToString() + " / " + h.Knife);
            }
            #endregion

            #region Exports
            ExcelExport objXLS = new ExcelExport();

            objXLS.WriteHdr(lstHdr);

            objXLS.WriteMults(lstMults);
            #endregion

            // testing
            Console.WriteLine("Press key to exit");
            Console.ReadKey();
        }
    }
}
