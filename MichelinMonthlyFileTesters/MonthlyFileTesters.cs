using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Data;
using FluentFTP;

namespace MichelinMonthlyFileTesters
{
    public class MonthlyFileTesters
    {
        //TODO
        // add test methods to  be called from each file test case
        //      number
        //      date
        //      line length
        //      done - " text qualifiers
        // add try catch blocks to each file test so that program will execute and log errors encountered instead of just executing
        //      add data logger to output log file for qa.
        // add correct column headers to refernce file?? 
        // refine runtime console logging. provide user with more information on what is going on. 
        // add recommendation to load / not load based on # of failed records



        // Michelin Monthly File Testing
        public static void MichelinMonthlyFileCleaner()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("This is to test the Monthly US files for Michelin.");
            Console.WriteLine();
            Console.WriteLine("Please be sure to only run this program from your desktop.");
            Console.ResetColor();

            string choice = string.Empty;
            while (choice != "exit" || choice != "Exit")
            {
                Console.WriteLine();
                Console.WriteLine("Enter Country you would like to test: ");
                Console.WriteLine("{0,5}{1,-10}", "1. ", "Test US Files");
                Console.WriteLine("{0,5}{1,-10}", "2. ", "Test Canada Files");
                Console.WriteLine();
                Console.WriteLine("\"exit\" - Closes the program.");
                Console.WriteLine();
                Console.Write("Your choice: ");

                choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        MichelinTestUSMonthlyFileFormats();
                        break;

                    case "2":
                        MichelinCanadaTestMontlyFileFormats();
                        break;

                    case "exit":
                        Environment.Exit(0);
                        break;
                }
            }
        }

        public static void MichelinTestUSMonthlyFileFormats() // michelin monthly file formatting.
        {

            // EDIT NOTES:
            // 12/5/18 - added line = line.Replace(",", string.Empty); to each while read line statement. this is to remove all , in the data to avoid conflicts in the e1 platform data formating.


            // add test to verify that the program is on a users desktop. notify user if this is not true. 
            //string path = Directory.GetCurrentDirectory();
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine();
            Console.WriteLine("This is to test the 8 Monthly US files for Michelin.");
            Console.WriteLine();
            Console.WriteLine("Please save all Michelin Monthly files to a folder on your DESKTOP.");
            Console.WriteLine("Please REMOVE ALL SPACES in the FILE NAMES and FILE PATHS.");
            Console.WriteLine("Please verify that ALL files are TAB DELIMITED.");
            Console.ResetColor();

            string path = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results";

            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
            }

            string choice = string.Empty;
            while (choice != "exit" || choice != "Exit")
            {
                Console.WriteLine();
                Console.WriteLine("Enter test number you would like to run: ");
                Console.WriteLine("{0,5}{1,-10}", "1. ", "Test ADD File.");
                Console.WriteLine("{0,5}{1,-10}", "2. ", "Test DIR File.");
                Console.WriteLine("{0,5}{1,-10}", "3. ", "Test Gap Price File.");
                Console.WriteLine("{0,5}{1,-10}", "4. ", "Test OM File.");
                Console.WriteLine("{0,5}{1,-10}", "5. ", "Test DT LAM Batch File.");
                Console.WriteLine("{0,5}{1,-10}", "6. ", "Test and Combine GAP Files");
                Console.WriteLine("{0,5}{1,-10}", "7. ", "KickOutMonth File Update.");
                Console.WriteLine();
                Console.WriteLine("\"exit\" - Closes the program.");
                Console.WriteLine();
                Console.Write("Your choice: ");

                choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        MichelinTestMonthlyFilesADD();
                        break;

                    case "2":
                        MichelinTestMonthlyFilesDIR();
                        break;

                    case "3":
                        MichelinTestMonthlyFilesGAPPrice();
                        break;

                    case "4":
                        MichelinTestMonthlyFilesOM();
                        break;

                    case "5":
                        MichelinDealerTireLAMMonthly();
                        break;

                    case "6":
                        MichelinTestAndCombineMonthlyGapFiles();
                        break;

                    case "7":
                        MonthlyKickoutFile();
                        break;

                    case "exit":
                        Environment.Exit(0);
                        break;
                }
            }
        }
        // individual tests.
        public static void MichelinTestMonthlyFilesADD()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("Please enter the ADD file.");
            string file = FunctionTools.GetAFile();
            string filename = FunctionTools.GetFileNameWithoutExtension(file);
            char del = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month first 3 letters(ex. JAN): ");
            string month = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string year = Console.ReadLine().Trim().ToUpper();

            month = month + year;

            string testedoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + filename + "_" + month + "_good_columns.txt";

            // error storage
            List<string> missingcolumnlist = new List<string>(); //save the list.
            HashSet<string> unmatchedfilecolumnlist = new HashSet<string>();
            Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
            int errorlinecount = 0;
            int errordelimitercount = 0;
            int blankvaluesadded = 0;

            //colum lists... they are in order of matching file -> table.
            string[] expectedfilecolumns = { "Plan Channel Member Group ID", "Plan Channel Member Group DESC", "Unique Customer AAD HQ# ID", "Unique Customer AAD HQ# DESC", "Unique Customer Class", "Unique Customer ID", "Unique Customer DESC", "Unique Customer Address 1",
                                               "Unique Customer State/Province", "Unique Customer City", "Unique Customer Zip/Postal", "Brand Group", "Year", "Month", "SELLOUT UNITS" };
            string[] newcolumns = { "PLAN_CHANNEL_MEMBER_GROUP_ID", "PLAN_CHANNEL_MEMBER_GROUP_DESC", "UNIQUE_CUSTOMER_AAD_HQ_ID", "UNIQUE_CUSTOMER_AAD_HQ_DESC", "UNIQUE_CUSTOMER_CLASS", "UNIQUE_CUSTOMER_ID", "UNIQUE_CUSTOMER_DESC", "UNIQUE_CUSTOMER_ADDRESS_1",
                                                "UNIQUE_CUSTOMER_STATE", "UNIQUE_CUSTOMER_CITY", "UNIQUE_CUSTOMER_ZIP", "BRAND", "YEAR", "MONTH", "SELLOUT_UNITS" };


            using (StreamReader readfile = new StreamReader(file))
            {
                using (StreamWriter passedtestfile = new StreamWriter(testedoutfile))
                {
                    //get header.
                    string header = readfile.ReadLine();
                    string[] headersplit = header.Split(del);
                    int linelength = expectedfilecolumns.Length;

                    //are all columns there? 
                    List<string> filecolumn = headersplit.ToList();
                    List<int> columnindexes = new List<int>();

                    foreach (var s in expectedfilecolumns)
                    {
                        if (filecolumn.Contains(s))
                        {
                            //save column index in order they appear in the expected column list - > order matches new header write order.
                            columnindexes.Add(FunctionTools.ColumnIndex(header, del, s));
                        }
                        else
                        {
                            missingcolumnlist.Add(s);
                            //Console.WriteLine("ADD File missing column - {0}", s);
                        }

                        if (missingcolumnlist.Count > 0)
                        {
                            foreach (var c in filecolumn)
                            {
                                if (!expectedfilecolumns.Contains(c))
                                {
                                    unmatchedfilecolumnlist.Add(c); // columns from file that were not matched.
                                }
                            }
                        }
                    }

                    //Column being checked indexes.
                    int uniquecustidcolumn = FunctionTools.ColumnIndex(header, del, "Unique Customer Id");
                    int yearcolumn = FunctionTools.ColumnIndex(header, del, "Year");
                    int monthcolumn = FunctionTools.ColumnIndex(header, del, "Month");

                    //write new header -> with new column headers that match the table to be reloaded.
                    string newheader = string.Join(del.ToString(), newcolumns);
                    passedtestfile.WriteLine(newheader);

                    // read rest of file and check columns.
                    string line = string.Empty;
                    int recordcount = 0;

                    while ((line = readfile.ReadLine()) != null)
                    {
                        //txtq replace
                        line = line.Replace("\"", "");

                        recordcount++; //starts at 1.

                        string[] splitline = line.Split(del);
                        int splitlength = splitline.Length;

                        while (splitlength > linelength)
                        {
                            //test line Length.
                            foreach (var value in splitline)
                            {
                                value.Trim();
                            }

                            if (line.Contains("\t\t"))
                            {
                                line = line.Replace("\t\t", "\t");
                                errordelimitercount++;
                            }
                            splitline = line.Split(del);
                            splitlength = splitline.Length;
                        }

                        //test column values.
                        List<string> failreport = new List<string>();

                        if (splitline.Length < headersplit.Length)
                        {
                            failreport.Add("Line does not have enough fields - " + line);
                        }
                        else
                        {

                            // unique customer id
                            int number = 0;
                            if (!int.TryParse(splitline[uniquecustidcolumn], out number))
                            {
                                failreport.Add("Unique Customer Id, not a number - " + splitline[uniquecustidcolumn]);
                            }

                            // year.
                            if (splitline[yearcolumn] != year)
                            {
                                failreport.Add("Year, does not match - " + splitline[yearcolumn]);
                            }

                            // month.
                            if (splitline[monthcolumn] != month)
                            {
                                failreport.Add("Month, does not match - " + splitline[monthcolumn]);
                            }
                        }


                        //outputs.
                        if (failreport.Count == 0)
                        {
                            List<string> newlinebuilder = new List<string>();
                            foreach (var val in columnindexes)
                            {
                                newlinebuilder.Add(splitline[val]);
                            }

                            if (splitlength < linelength) //if the line is not long enough. we just add the value.
                            {
                                while (splitlength < linelength)
                                {
                                    newlinebuilder.Add(string.Empty);

                                    //reassign array values
                                    splitline = newlinebuilder.ToArray();
                                    blankvaluesadded++;
                                    splitlength = splitline.Length;
                                }
                            }

                            passedtestfile.WriteLine(string.Join(del.ToString(), newlinebuilder.ToArray()));
                        }
                        else
                        {
                            errorlinecount++; // there are errors in this line.
                            failedrecordsdict.Add(recordcount, failreport);
                        }
                    }
                }

                // test results
                MichelinMonthlyFilesTestReport(file, testedoutfile, missingcolumnlist, unmatchedfilecolumnlist, failedrecordsdict, errorlinecount, errordelimitercount, blankvaluesadded);

            }
        }

        public static void MichelinTestMonthlyFilesDIR()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("Please enter the DIR file.");
            string file = FunctionTools.GetAFile();
            string filename = FunctionTools.GetFileNameWithoutExtension(file);
            char del = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month first 3 letters(ex. JAN): ");
            string month = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string year = Console.ReadLine().Trim().ToUpper();

            month = month + year;

            string testedoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + filename + "_" + month + "_good_columns.txt";

            // error storage
            List<string> missingcolumnlist = new List<string>(); //save the list.
            HashSet<string> unmatchedfilecolumnlist = new HashSet<string>();
            Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
            int errorlinecount = 0;
            int errordelimitercount = 0;
            int blankvaluesadded = 0;

            //colum lists... they are in order of matching file -> table.
            string[] expectedfilecolumns = { "Country", "Level 2 Sales Position ID", "Level 2 Sales Position Name", "Plan Channel Member Group ID", "Plan Channel Member Group DESC", "Bill To Customer ID", "Bill To Customer DESC", "Bill To Cust Chnl ID", "Bill To Cust Chnl DESC", "Ship To Number #", "Ship To Number DESC", "Ship To Cust Chnl ID", "Ship To Cust Chnl DESC", "Ship To Address 1", "Ship To City", "Ship To State/Province", "Ship To Zip/Postal", "Brand Group", "Year", "Month", "Ship To Phone Number", "NAR UNITS", "SELLOUT UNITS w/o BIB EXP for NTW" };

            string[] newcolumns = { "COUNTRY_DESC", "LEVEL_2_SALES_POSITION_ID", "LEVEL_2_SALES_POSITION_NAME", "PLAN_CHANNEL_MEMBER_GROUP_ID", "PLAN_CHANNEL_MEMBER_GROUP_DESC", "BILL_TO_CUSTOMER_ID", "BILL_TO_CUSTOMER_DESC", "BILL_TO_CUST_CHNL_ID", "BILL_TO_CUST_CHNL_DESC", "SHIP_TO_NUMBER_ID", "SHIP_TO_NUMBER_DESC", "SHIP_TO_CUST_CHNL_ID", "SHIP_TO_CUST_CHNL_DESC", "SHIP_TO_ADDRESS_1_ID", "SHIP_TO_CITY_ID", "SHIP_TO_STATE", "SHIP_TO_ZIP", "BRAND", "YEAR", "MONTH", "UNK1", "NAR_UNITS", "SELLOUT_UNITS" };

            using (StreamReader readfile = new StreamReader(file))
            {
                using (StreamWriter passedtestfile = new StreamWriter(testedoutfile))
                {
                    //get header.
                    string header = readfile.ReadLine();
                    string[] headersplit = header.Split(del);
                    int linelength = headersplit.Length;

                    //Are all the columns there? 
                    List<string> filecolumn = headersplit.ToList();
                    List<int> columnindexes = new List<int> { };

                    foreach (var s in expectedfilecolumns)
                    {
                        if (filecolumn.Contains(s))
                        {
                            //save column index in order they appear in the expected column list - > order matches new header write order.
                            columnindexes.Add(FunctionTools.ColumnIndex(header, del, s));
                        }
                        else
                        {
                            missingcolumnlist.Add(s);
                            //Console.WriteLine("ADD File missing column - {0}", s);
                        }

                        if (missingcolumnlist.Count > 0)
                        {
                            foreach (var c in filecolumn)
                            {
                                if (!expectedfilecolumns.Contains(c))
                                {
                                    unmatchedfilecolumnlist.Add(c); // columns from file that were not matched.
                                }
                            }
                        }
                    }

                    //Column being checked indexes.
                    int monthcolumn = FunctionTools.ColumnIndex(header, del, "Month".ToUpper());
                    int yearcolumn = FunctionTools.ColumnIndex(header, del, "Year".ToUpper());

                    int billcustomerid = FunctionTools.ColumnIndex(header, del, "Bill To Customer Id".ToUpper());
                    int billcustomerchannelid = FunctionTools.ColumnIndex(header, del, "Bill To Cust Chnl Id".ToUpper());
                    int shiptonumberid = FunctionTools.ColumnIndex(header, del, "Ship To Number #".ToUpper());
                    int shiptocustomerchnlid = FunctionTools.ColumnIndex(header, del, "Ship To Cust Chnl Id".ToUpper());
                    int narunits = FunctionTools.ColumnIndex(header, del, "Nar Units".ToUpper());
                    int selloutunits = FunctionTools.ColumnIndex(header, del, "SELLOUT UNITS w/o BIB EXP for NTW".ToUpper());

                    List<int> numbercolumnslist = new List<int> { billcustomerchannelid, billcustomerid, shiptocustomerchnlid, shiptonumberid, narunits, selloutunits };

                    //write new header -> with new column headers that match the table to be reloaded.
                    string newheader = string.Join(del.ToString(), newcolumns);
                    passedtestfile.WriteLine(newheader);

                    // read rest of file and check columns.
                    string line = string.Empty;
                    int recordcount = 0;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        //txtq replace
                        line = line.Replace("\"", "");

                        recordcount++; //starts at 1.

                        string[] splitline = line.Split(del);
                        int splitlength = splitline.Length;

                        while (splitlength > linelength)
                        {
                            //test line Length.
                            foreach (var value in splitline)
                            {
                                value.Trim();
                            }

                            if (line.Contains("\t\t"))
                            {
                                line = line.Replace("\t\t", "\t");
                                errordelimitercount++;
                            }
                            splitline = line.Split(del);
                            splitlength = splitline.Length;
                        }

                        //test column values.
                        List<string> failreport = new List<string>();

                        if (splitline.Length < headersplit.Length)
                        {
                            failreport.Add("Line does not have enough fields - " + line);
                        }
                        else
                        {
                            //number columns
                            foreach (var index in numbercolumnslist)
                            {
                                int number = 0;
                                if (!string.IsNullOrWhiteSpace(splitline[index]))
                                {
                                    if (!int.TryParse(splitline[index], out number))
                                    {
                                        failreport.Add(newcolumns[index] + ", not a number - " + splitline[index]);
                                    }
                                }
                            }

                            // year.
                            if (splitline[yearcolumn] != year)
                            {
                                failreport.Add("Year, does not match - " + splitline[yearcolumn]);
                            }

                            // month.
                            if (splitline[monthcolumn] != month)
                            {
                                failreport.Add("Month, does not match - " + splitline[monthcolumn]);
                            }
                        }

                        //outputs.
                        if (failreport.Count == 0)
                        {
                            List<string> newlinebuilder = new List<string>();
                            foreach (var val in columnindexes)
                            {
                                newlinebuilder.Add(splitline[val]);
                            }

                            if (splitlength < linelength) //if the line is not long enough. we just add the value.
                            {
                                while (splitlength < linelength)
                                {
                                    newlinebuilder.Add(string.Empty);

                                    //reassign array values
                                    splitline = newlinebuilder.ToArray();
                                    blankvaluesadded++;
                                    splitlength = splitline.Length;
                                }
                            }

                            passedtestfile.WriteLine(string.Join(del.ToString(), newlinebuilder.ToArray()));
                        }
                        else
                        {
                            errorlinecount++; // there are errors in this line.
                            failedrecordsdict.Add(recordcount, failreport);
                        }
                    }

                    // test results
                    MichelinMonthlyFilesTestReport(file, testedoutfile, missingcolumnlist, unmatchedfilecolumnlist, failedrecordsdict, errorlinecount, errordelimitercount, blankvaluesadded);

                }
            }
        }

        public static void MichelinTestMonthlyFilesGAPPrice()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("Please enter the GAP PRICE file.");
            string file = FunctionTools.GetAFile();
            string filename = FunctionTools.GetFileNameWithoutExtension(file);
            char del = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month number (ex. 01): ");
            string month = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string year = Console.ReadLine().Trim().ToUpper();

            month = year + month;

            string testedoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + filename + "_" + month + "_good_columns.txt";

            // error storage
            List<string> missingcolumnlist = new List<string>(); //save the list.
            HashSet<string> unmatchedfilecolumnlist = new HashSet<string>();
            Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
            int errorlinecount = 0;
            int errordelimitercount = 0;
            int blankvaluesadded = 0;

            string[] expectedfilecolumns = { "Bill To Number", "MSPN", "Month", "AVG NET NET PRICE AMT", "AVG INVC PRICE AMT" };
            string[] newcolumns = { "BILL_TO_NUMBER", "MSPN", "MONTH", "AVG_NET_NET_PRICE_AMT", "AVG_INVC_PRICE_AMT" };

            using (StreamReader readfile = new StreamReader(file))
            {
                using (StreamWriter passedtestfile = new StreamWriter(testedoutfile))
                {
                    //get header.
                    string header = readfile.ReadLine();
                    string[] headersplit = header.Split(del);
                    int linelength = headersplit.Length;

                    //are all columns there? 
                    List<string> filecolumn = headersplit.ToList();
                    List<int> columnindexes = new List<int>();

                    foreach (var s in expectedfilecolumns)
                    {
                        if (filecolumn.Contains(s))
                        {
                            //save column index in order they appear in the expected column list - > order matches new header write order.
                            columnindexes.Add(FunctionTools.ColumnIndex(header, del, s));
                        }
                        else
                        {
                            missingcolumnlist.Add(s);
                            //Console.WriteLine("ADD File missing column - {0}", s);
                        }

                        if (missingcolumnlist.Count > 0)
                        {
                            foreach (var c in filecolumn)
                            {
                                if (!expectedfilecolumns.Contains(c))
                                {
                                    unmatchedfilecolumnlist.Add(c); // columns from file that were not matched.
                                }
                            }
                        }
                    }


                    //Column being checked indexes.
                    int monthcolumn = FunctionTools.ColumnIndex(header, del, "Month".ToUpper());

                    //numbertests.
                    int avgnetnetprice = FunctionTools.ColumnIndex(header, del, "AVG NET NET PRICE AMT".ToUpper());
                    int avginvcprice = FunctionTools.ColumnIndex(header, del, "AVG INVC PRICE AMT".ToUpper());
                    List<int> numbercolumnslist = new List<int> { avginvcprice, avgnetnetprice };

                    //write new header -> with new column headers that match the table to be reloaded.
                    string newheader = string.Join(del.ToString(), newcolumns);
                    passedtestfile.WriteLine(newheader);

                    // read rest of file and check columns.
                    string line = string.Empty;
                    int recordcount = 0;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        //txtq replace
                        line = line.Replace("\"", "");

                        recordcount++; //starts at 1.

                        string[] splitline = line.Split(del);
                        int splitlength = splitline.Length;

                        while (splitlength > linelength)
                        {
                            //test line Length.
                            foreach (var value in splitline)
                            {
                                value.Trim();
                            }

                            if (line.Contains("\t\t"))
                            {
                                line = line.Replace("\t\t", "\t");
                                errordelimitercount++;
                            }
                            splitline = line.Split(del);
                            splitlength = splitline.Length;
                        }


                        //test column values.
                        List<string> failreport = new List<string>();

                        if (splitline.Length < headersplit.Length)
                        {
                            failreport.Add("Line does not have enough fields - " + line);
                        }
                        else
                        {
                            //number columns
                            foreach (var index in numbercolumnslist)
                            {
                                decimal number = 0;
                                if (!string.IsNullOrWhiteSpace(splitline[index]))
                                {
                                    if (!decimal.TryParse(splitline[index], out number))
                                    {
                                        failreport.Add(newcolumns[index] + ", not a number - " + splitline[index]);
                                    }
                                }
                            }

                            // month.
                            if (splitline[monthcolumn] != month)
                            {
                                failreport.Add("Month, does not match - " + splitline[monthcolumn]);
                            }

                        }
                        //outputs.
                        if (failreport.Count == 0)
                        {
                            List<string> newlinebuilder = new List<string>();
                            foreach (var val in columnindexes)
                            {
                                newlinebuilder.Add(splitline[val]);
                            }

                            if (splitlength < linelength) //if the line is not long enough. we just add the value.
                            {
                                while (splitlength < linelength)
                                {
                                    newlinebuilder.Add(string.Empty);

                                    //reassign array values
                                    splitline = newlinebuilder.ToArray();
                                    blankvaluesadded++;
                                    splitlength = splitline.Length;
                                }
                            }

                            passedtestfile.WriteLine(string.Join(del.ToString(), newlinebuilder.ToArray()));
                        }
                        else
                        {
                            errorlinecount++; // there are errors in this line.
                            failedrecordsdict.Add(recordcount, failreport);
                        }
                    }

                    // test results
                    MichelinMonthlyFilesTestReport(file, testedoutfile, missingcolumnlist, unmatchedfilecolumnlist, failedrecordsdict, errorlinecount, errordelimitercount, blankvaluesadded);
                }
            }
        }

        public static void MichelinTestMonthlyFilesOM()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("Please enter the OM file.");
            string file = FunctionTools.GetAFile();
            string filename = FunctionTools.GetFileNameWithoutExtension(file);
            char del = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month first 3 letters(ex. JAN): ");
            string month = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string year = Console.ReadLine().Trim().ToUpper();

            month = month + year;

            string testedoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + filename + "_" + month + "_good_columns.txt";

            // error storage
            List<string> missingcolumnlist = new List<string>(); //save the list.
            HashSet<string> unmatchedfilecolumnlist = new HashSet<string>();
            Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
            int errorlinecount = 0;
            int errordelimitercount = 0;
            int blankvaluesadded = 0;

            //colum lists... they are in order of matching file -> table.
            string[] expectedfilecolumns = { "Plan Channel Member Group ID", "Plan Channel Member Group DESC", "Unique Customer Class", "Unique Customer ID", "Unique Customer DESC", "Unique Customer Address 1", "Unique Customer State/Province", "Unique Customer City", "Unique Customer Zip/Postal", "Brand Group", "Year", "Month", "SELLOUT UNITS" };
            string[] newcolumns = { "PLAN_CHANNEL_MEMBER_GROUP_ID", "PLAN_CHANNEL_MEMBER_GROUP_DESC", "UNIQUE_CUSTOMER_CLASS", "UNIQUE_CUSTOMER_ID", "UNIQUE_CUSTOMER_DESC", "UNIQUE_CUSTOMER_ADDRESS_1", "UNIQUE_CUSTOMER_STATE", "UNIQUE_CUSTOMER_CITY", "UNIQUE_CUSTOMER_ZIP", "BRAND", "YEAR", "MONTH", "SELLOUT_UNITS" };

            using (StreamReader readfile = new StreamReader(file))
            {
                using (StreamWriter passedtestfile = new StreamWriter(testedoutfile))
                {
                    //get header.
                    string header = readfile.ReadLine();
                    string[] headersplit = header.Split(del);
                    int linelength = headersplit.Length;

                    //are all columns there? 
                    List<string> filecolumn = headersplit.ToList();
                    List<int> columnindexes = new List<int> { };

                    foreach (var s in expectedfilecolumns)
                    {
                        if (filecolumn.Contains(s))
                        {
                            //save column index in order they appear in the expected column list - > order matches new header write order.
                            columnindexes.Add(FunctionTools.ColumnIndex(header, del, s));
                        }
                        else
                        {
                            missingcolumnlist.Add(s);
                            //Console.WriteLine("ADD File missing column - {0}", s);
                        }

                        if (missingcolumnlist.Count > 0)
                        {
                            foreach (var c in filecolumn)
                            {
                                if (!expectedfilecolumns.Contains(c))
                                {
                                    unmatchedfilecolumnlist.Add(c); // columns from file that were not matched.
                                }
                            }
                        }
                    }

                    //Column being checked indexes.
                    int uniquecustidcolumn = FunctionTools.ColumnIndex(header, del, "Unique Customer Id");
                    int yearcolumn = FunctionTools.ColumnIndex(header, del, "Year");
                    int monthcolumn = FunctionTools.ColumnIndex(header, del, "Month");


                    //write new header -> with new column headers that match the table to be reloaded.
                    string newheader = string.Join(del.ToString(), newcolumns);
                    passedtestfile.WriteLine(newheader);

                    // read rest of file and check columns.
                    string line = string.Empty;
                    int recordcount = 0;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        //txtq replace
                        line = line.Replace("\"", "");

                        recordcount++; //starts at 1.

                        string[] splitline = line.Split(del);
                        int splitlength = splitline.Length;

                        while (splitlength > linelength)
                        {
                            //test line Length.
                            foreach (var value in splitline)
                            {
                                value.Trim();
                            }

                            if (line.Contains("\t\t"))
                            {
                                line = line.Replace("\t\t", "\t");
                                errordelimitercount++;
                            }
                            splitline = line.Split(del);
                            splitlength = splitline.Length;
                        }

                        //test column values.
                        List<string> failreport = new List<string>();

                        if (splitline.Length < headersplit.Length)
                        {
                            failreport.Add("Line does not have enough fields - " + line);
                        }
                        else
                        {
                            // unique customer id
                            int number = 0;
                            if (!int.TryParse(splitline[uniquecustidcolumn], out number))
                            {
                                failreport.Add("Unique Customer Id, not a number - " + splitline[uniquecustidcolumn]);
                            }

                            // year.
                            if (splitline[yearcolumn] != year)
                            {
                                failreport.Add("Year, does not match - " + splitline[yearcolumn]);
                            }

                            // month.
                            if (splitline[monthcolumn] != month)
                            {
                                failreport.Add("Month, does not match - " + splitline[monthcolumn]);
                            }


                        }

                        //outputs.
                        if (failreport.Count == 0)
                        {
                            List<string> newlinebuilder = new List<string>();
                            foreach (var val in columnindexes)
                            {
                                newlinebuilder.Add(splitline[val]);
                            }

                            if (splitlength < linelength) //if the line is not long enough. we just add the value.
                            {
                                while (splitlength < linelength)
                                {
                                    newlinebuilder.Add(string.Empty);

                                    //reassign array values
                                    splitline = newlinebuilder.ToArray();
                                    blankvaluesadded++;
                                    splitlength = splitline.Length;
                                }
                            }

                            passedtestfile.WriteLine(string.Join(del.ToString(), newlinebuilder.ToArray()));
                        }
                        else
                        {
                            errorlinecount++; // there are errors in this line.
                            failedrecordsdict.Add(recordcount, failreport);
                        }
                    }

                    // test results
                    MichelinMonthlyFilesTestReport(file, testedoutfile, missingcolumnlist, unmatchedfilecolumnlist, failedrecordsdict, errorlinecount, errordelimitercount, blankvaluesadded);
                }
            }
        }

        public static void MichelinDealerTireLAMMonthly()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("Please enter the US Dealer Tire Lam Batch file.");
            string file = FunctionTools.GetAFile();
            string filename = FunctionTools.GetFileNameWithoutExtension(file);
            char del = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month first 3 letters(ex. JAN): ");
            string month = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string year = Console.ReadLine().Trim().ToUpper();

            month = month + year;

            string testedoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + filename + "_" + month + "_good_columns.txt";

            // error storage
            List<string> missingcolumnlist = new List<string>(); //save the list.
            HashSet<string> unmatchedfilecolumnlist = new HashSet<string>();
            Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
            int errorlinecount = 0;
            int errordelimitercount = 0;
            int blankvaluesadded = 0;

            //colum lists... they are in order of matching file -> table.
            string[] expectedfilecolumns = { "Plan Channel Member Group ID", "Plan Channel Member Group DESC", "Indirect Customer Indirect Customer ID", "Indirect Customer DESC", "Indirect Customer Address 1", "Indirect Customer State/Province", "Indirect Customer City", "Indirect Customer Zip/Postal", "Brand", "Year", "Month", "SELLOUT UNITS" };
            string[] newcolumns = { "PLAN_CHANNEL_MEMBER_GROUP_ID", "PLAN_CHANNEL_MEMBER_GROUP_DESC", "UNIQUE_CUSTOMER_ID", "UNIQUE_CUSTOMER_DESC", "UNIQUE_CUSTOMER_ADDRESS_1", "UNIQUE_CUSTOMER_STATE", "UNIQUE_CUSTOMER_CITY", "UNIQUE_CUSTOMER_ZIP", "BRAND", "YEAR", "MONTH", "SELLOUT_UNITS" };

            using (StreamReader readfile = new StreamReader(file))
            {
                using (StreamWriter passedtestfile = new StreamWriter(testedoutfile))
                {
                    //get header.
                    string header = readfile.ReadLine();
                    string[] headersplit = header.Split(del);
                    int linelength = headersplit.Length;

                    //are all columns there? 
                    List<string> filecolumn = headersplit.ToList();
                    List<int> columnindexes = new List<int> { };

                    foreach (var s in expectedfilecolumns)
                    {
                        if (filecolumn.Contains(s))
                        {
                            //save column index in order they appear in the expected column list - > order matches new header write order.
                            columnindexes.Add(FunctionTools.ColumnIndex(header, del, s));
                        }
                        else
                        {
                            missingcolumnlist.Add(s);
                            //Console.WriteLine("ADD File missing column - {0}", s);
                        }

                        if (missingcolumnlist.Count > 0)
                        {
                            foreach (var c in filecolumn)
                            {
                                if (!expectedfilecolumns.Contains(c))
                                {
                                    unmatchedfilecolumnlist.Add(c); // columns from file that were not matched.
                                }
                            }
                        }
                    }

                    //Column being checked indexes.
                    //int uniquecustidcolumn = ColumnIndex(usdtlamheader, usdtlamdel, "Plan Channel Member Group ID");
                    int yearcolumn = FunctionTools.ColumnIndex(header, del, "Year");
                    int monthcolumn = FunctionTools.ColumnIndex(header, del, "Month");

                    //write new header -> with new column headers that match the table to be reloaded.
                    string newheader = string.Join(del.ToString(), newcolumns);
                    passedtestfile.WriteLine(newheader);

                    // read rest of file and check columns.
                    string line = string.Empty;
                    int recordcount = 0;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        //txtq replace
                        line = line.Replace("\"", "");

                        recordcount++; //starts at 1.
                                       //line = line.Replace(",", string.Empty);
                        string[] splitline = line.Split(del);
                        int splitlength = splitline.Length;

                        while (splitlength > linelength)
                        {
                            //test line Length.
                            foreach (var value in splitline)
                            {
                                value.Trim();
                            }

                            if (line.Contains("\t\t"))
                            {
                                line = line.Replace("\t\t", "\t");
                                errordelimitercount++;
                            }
                            splitline = line.Split(del);
                            splitlength = splitline.Length;
                        }

                        //test column values.
                        List<string> failreport = new List<string>();

                        if (splitline.Length < headersplit.Length)
                        {
                            failreport.Add("Line does not have enough fields - " + line);
                        }
                        else
                        {

                            // year.
                            if (splitline[yearcolumn] != year)
                            {
                                failreport.Add("Year, does not match - " + splitline[yearcolumn]);
                            }

                            // month.
                            if (splitline[monthcolumn] != month)
                            {
                                failreport.Add("Month, does not match - " + splitline[monthcolumn]);
                            }
                        }


                        //outputs.
                        if (failreport.Count == 0)
                        {
                            List<string> newlinebuilder = new List<string>();
                            foreach (var val in columnindexes)
                            {
                                newlinebuilder.Add(splitline[val]);
                            }

                            if (splitlength < linelength) //if the line is not long enough. we just add the value.
                            {
                                while (splitlength < linelength)
                                {
                                    newlinebuilder.Add(string.Empty);

                                    //reassign array values
                                    splitline = newlinebuilder.ToArray();
                                    blankvaluesadded++;
                                    splitlength = splitline.Length;
                                }
                            }

                            passedtestfile.WriteLine(string.Join(del.ToString(), newlinebuilder.ToArray()));
                        }
                        else
                        {
                            errorlinecount++; // there are errors in this line.
                            failedrecordsdict.Add(recordcount, failreport);
                        }
                    }

                    // test results
                    MichelinMonthlyFilesTestReport(file, testedoutfile, missingcolumnlist, unmatchedfilecolumnlist, failedrecordsdict, errorlinecount, errordelimitercount, blankvaluesadded);
                }
            }
        }

        public static void MichelinTestAndCombineMonthlyGapFiles()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("This will test and combine all three US GAP files provided by Michelin.");
            Console.WriteLine();

            Console.WriteLine("Please enter the US Dealer Tire GAP Monthly Batch File.");
            string gapbatchfile = FunctionTools.GetAFile();
            char gapbatchdel = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month first 3 letters(ex. JAN): ");
            string gapbatchmonth = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the file Month number (ex. 01): ");
            string gapbatchmonthnumber = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string gapbatchyear = Console.ReadLine().Trim().ToUpper();

            Console.WriteLine();
            Console.WriteLine("------------------------------------------------------------------------------------");
            Console.WriteLine("Please enter the GAP US TCAR PS Extract File.");
            string gaptcarfile = FunctionTools.GetAFile();
            char gaptcardel = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month number (ex. 01): ");
            string gaptcarmonth = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string gaptcaryear = Console.ReadLine().Trim().ToUpper();

            Console.WriteLine();
            Console.WriteLine("------------------------------------------------------------------------------------");
            Console.WriteLine("Please enter the MSPN ST AAD US OMA For ZONE LAM File.");
            string mspnfile = FunctionTools.GetAFile();
            char mspndel = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month number (ex. 01): ");
            string mspnmonth = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string mspnyear = Console.ReadLine().Trim().ToUpper();

            //new file
            string testedoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + "USGAPFILE_" + gapbatchmonth + "_good_columns.txt";
            char passedtestfiledel = '|';

            // platform table column order. output file will have these columns.
            string[] platformtablecolumns = { "ST_CUST_NBR", "MSPN_NBR", "CALENDAR_CCYYMM_NBR", "NET_SALES_UNITS", "SELLOUT_UNITS", "NET_INVC_UNITS", "BIB_X_UNITS" };

            using (StreamWriter passedtestfile = new StreamWriter(testedoutfile))
            {
                //write header
                passedtestfile.WriteLine(string.Join(passedtestfiledel.ToString(), platformtablecolumns));

                //gap batch file
                using (StreamReader readfile = new StreamReader(gapbatchfile))
                {
                    // error storage
                    List<string> missingcolumnlist = new List<string>(); //save the list.
                    HashSet<string> unmatchedfilecolumnlist = new HashSet<string>();
                    Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
                    int errorlinecount = 0;
                    int errordelimitercount = 0;
                    int blankvaluesadded = 0;

                    //get header.
                    string header = readfile.ReadLine();
                    string[] headersplit = header.Split(gapbatchdel);
                    int linelength = headersplit.Length;

                    //are all columns there? 
                    List<string> filecolumn = headersplit.ToList();
                    List<int> columnindexes = new List<int>();

                    //string[] expectedfilecolumns = { "Indirect Customer", "MSPN", "Month", "Year", "SELLOUT UNITS" }; //expected column order
                    string[] expectedfilecolumns = { "Indirect Customer", "MSPN", "Month", "SELLOUT UNITS" };
                    string[] newcolumns = { "ST_CUST_NBR", "MSPN_NBR", "CALENDAR_CCYYMM_NBR", "SELLOUT_UNITS" };

                    //old MMMyyyy
                    gapbatchmonth = gapbatchmonth + gapbatchyear;
                    //new month format
                    gapbatchmonthnumber = gapbatchyear + gapbatchmonthnumber;

                    foreach (var column in expectedfilecolumns)
                    {
                        if (filecolumn.Contains(column))
                        {
                            //save column index in order they appear in the expected column list - > order matches new header write order.
                            columnindexes.Add(FunctionTools.ColumnIndex(header, gapbatchdel, column));
                        }
                        else
                        {
                            missingcolumnlist.Add(column);
                            //Console.WriteLine("ADD File missing column - {0}", s);
                        }

                        if (missingcolumnlist.Count > 0)
                        {
                            foreach (var c in filecolumn)
                            {
                                if (!expectedfilecolumns.Contains(c))
                                {
                                    unmatchedfilecolumnlist.Add(c); // columns from file that were not matched.
                                }
                            }
                        }
                    }

                    if (columnindexes.Count != expectedfilecolumns.Length)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0} - File does not contain all expected columns. Please check file headers then restart this process.", FunctionTools.GetFileNameWithoutExtension(gapbatchfile));
                        Console.WriteLine("************************************************************************************");
                        Console.WriteLine("Expected Columns: ");
                        foreach (var column in expectedfilecolumns)
                        {
                            Console.WriteLine(column);
                        }

                        Console.ResetColor();

                        MichelinMonthlyFileCleaner();
                    }

                    //Column being checked indexes.
                    int monthcolumn = FunctionTools.ColumnIndex(header, gapbatchdel, "Month");

                    // read rest of file and check columns.
                    string line = string.Empty;
                    int recordcount = 0;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        //txtq replace
                        line = line.Replace("\"", "");

                        recordcount++; //starts at 1.

                        string[] splitline = line.Split(gapbatchdel);
                        int splitlength = splitline.Length;

                        while (splitlength > linelength)
                        {
                            //test line Length.
                            foreach (var value in splitline)
                            {
                                value.Trim();
                            }

                            if (line.Contains("\t\t"))
                            {
                                line = line.Replace("\t\t", "\t");
                                errordelimitercount++;
                            }
                            splitline = line.Split(gapbatchdel);
                            splitlength = splitline.Length;
                        }

                        //test column values.
                        List<string> failreport = new List<string>();

                        // month.
                        if (splitline[monthcolumn] == gapbatchmonth || splitline[monthcolumn] == gapbatchmonthnumber)
                        {
                            //hard set change to user entered values.
                            splitline[monthcolumn] = gapbatchmonthnumber;
                        }
                        else
                        {
                            failreport.Add("Month, does not match - " + splitline[monthcolumn]);
                        }

                        //outputs.
                        if (failreport.Count == 0)
                        {
                            //write to passtedtestfile, but have to match indexes first.
                            string[] newlinevaluearray = new string[platformtablecolumns.Length];

                            for (int i = 0; i <= newcolumns.Length - 1; i++)
                            {
                                foreach (var platformcolumn in platformtablecolumns)
                                {
                                    if (newcolumns[i] == platformcolumn)
                                    {
                                        //add value with index saved in columnindexes.
                                        int platformheadercolumnindex = Array.IndexOf(platformtablecolumns, platformcolumn);

                                        newlinevaluearray[platformheadercolumnindex] = splitline[columnindexes[i]];
                                    }
                                }
                            }

                            passedtestfile.WriteLine(string.Join(passedtestfiledel.ToString(), newlinevaluearray));
                        }
                        else
                        {
                            errorlinecount++; // there are errors in this line.
                            failedrecordsdict.Add(recordcount, failreport);
                        }
                    }

                    // test results
                    MichelinMonthlyFilesTestReport(gapbatchfile, testedoutfile, missingcolumnlist, unmatchedfilecolumnlist, failedrecordsdict, errorlinecount, errordelimitercount, blankvaluesadded);
                }

                //gap tcar file
                using (StreamReader readfile = new StreamReader(gaptcarfile))
                {
                    // error storage
                    List<string> missingcolumnlist = new List<string>(); //save the list.
                    HashSet<string> unmatchedfilecolumnlist = new HashSet<string>();
                    Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
                    int errorlinecount = 0;
                    int errordelimitercount = 0;
                    int blankvaluesadded = 0;

                    //get header.
                    string header = readfile.ReadLine();
                    string[] headersplit = header.Split(gapbatchdel);
                    int linelength = headersplit.Length;

                    //are all columns there? 
                    List<string> filecolumn = headersplit.ToList();
                    List<int> columnindexes = new List<int>();

                    string[] expectedfilecolumns = { "Ship To Number", "MSPN", "Month", "NET SLS UNITS w NTW Bibx", "SELLOUT UNITS w/o BIB EXP for NTW", "NET INVC UNITS", "BIB EXPRESS TS UNITS Plus" };
                    string[] newcolumns = { "ST_CUST_NBR", "MSPN_NBR", "CALENDAR_CCYYMM_NBR", "NET_SALES_UNITS", "SELLOUT_UNITS", "NET_INVC_UNITS", "BIB_X_UNITS" };

                    gaptcarmonth = gaptcaryear + gaptcarmonth;

                    foreach (var s in expectedfilecolumns)
                    {
                        if (filecolumn.Contains(s))
                        {
                            //save column index in order they appear in the expected column list - > order matches new header write order.
                            columnindexes.Add(FunctionTools.ColumnIndex(header, gaptcardel, s));
                        }
                        else
                        {
                            missingcolumnlist.Add(s);
                            //Console.WriteLine("ADD File missing column - {0}", s);
                        }

                        if (missingcolumnlist.Count > 0)
                        {
                            foreach (var c in filecolumn)
                            {
                                if (!expectedfilecolumns.Contains(c))
                                {
                                    unmatchedfilecolumnlist.Add(c); // columns from file that were not matched.
                                }
                            }
                        }
                    }

                    if (columnindexes.Count != expectedfilecolumns.Length)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0} - File does not contain all expected columns. Please check file headers then restart this process.", FunctionTools.GetFileNameWithoutExtension(gaptcarfile));
                        Console.WriteLine("************************************************************************************");
                        Console.WriteLine("Expected Columns: ");
                        foreach (var column in expectedfilecolumns)
                        {
                            Console.WriteLine(column);
                        }
                        Console.ResetColor();

                        MichelinMonthlyFileCleaner();
                    }

                    //Column being checked indexes.
                    int monthcolumn = FunctionTools.ColumnIndex(header, gaptcardel, "Month");

                    //number test.
                    int etslsunitscolumn = FunctionTools.ColumnIndex(header, gaptcardel, "ET SLS UNITS w NTW Bibx".ToUpper());
                    int selloutunitscolumn = FunctionTools.ColumnIndex(header, gaptcardel, "SELLOUT UNITS w/o BIB EXP for NTW".ToUpper());
                    int netinvcunitscolumn = FunctionTools.ColumnIndex(header, gaptcardel, "NET INVC UNITS".ToUpper());
                    int bibexpresscolumn = FunctionTools.ColumnIndex(header, gaptcardel, "BIB EXPRESS TS UNITS".ToUpper());

                    List<int> numbercolumnslist = new List<int> { etslsunitscolumn, selloutunitscolumn, netinvcunitscolumn, bibexpresscolumn };

                    // read rest of file and check columns.
                    string line = string.Empty;
                    int recordcount = 0;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        recordcount++; //starts at 1.

                        string[] splitline = line.Split(gaptcardel);
                        int splitlength = splitline.Length;

                        while (splitlength > linelength)
                        {
                            //test line Length.
                            foreach (var value in splitline)
                            {
                                value.Trim();
                            }

                            if (line.Contains("\t\t"))
                            {
                                line = line.Replace("\t\t", "\t");
                                errordelimitercount++;
                            }
                            splitline = line.Split(gaptcardel);
                            splitlength = splitline.Length;
                        }

                        //test column values.
                        List<string> failreport = new List<string>();

                        foreach (var index in numbercolumnslist)
                        {
                            int number = 0;
                            if (!string.IsNullOrWhiteSpace(splitline[index]))
                            {
                                if (!int.TryParse(splitline[index].Replace(",", string.Empty), out number))
                                {
                                    failreport.Add(newcolumns[index] + ", not a number - " + splitline[index]);
                                }
                            }
                        }

                        // month.
                        if (splitline[monthcolumn] != gaptcarmonth) //test both formats.
                        {
                            failreport.Add("Month, does not match - " + splitline[monthcolumn]);
                        }

                        //outputs.
                        if (failreport.Count == 0)
                        {
                            //write to passtedtestfile, but have to match indexes first.
                            string[] newlinevaluearray = new string[platformtablecolumns.Length];

                            for (int i = 0; i <= newcolumns.Length - 1; i++)
                            {
                                foreach (var platformcolumn in platformtablecolumns)
                                {
                                    if (newcolumns[i] == platformcolumn)
                                    {
                                        //add value with index saved in columnindexes.
                                        int platformheadercolumnindex = Array.IndexOf(platformtablecolumns, platformcolumn);

                                        newlinevaluearray[platformheadercolumnindex] = splitline[columnindexes[i]];
                                    }
                                }
                            }

                            passedtestfile.WriteLine(string.Join(passedtestfiledel.ToString(), newlinevaluearray));
                        }
                        else
                        {
                            errorlinecount++; // there are errors in this line.
                            failedrecordsdict.Add(recordcount, failreport);
                        }
                    }

                    // test results
                    MichelinMonthlyFilesTestReport(gaptcarfile, testedoutfile, missingcolumnlist, unmatchedfilecolumnlist, failedrecordsdict, errorlinecount, errordelimitercount, blankvaluesadded);
                }

                //MSPN ST AAD US OMA For ZONE LAM
                using (StreamReader readfile = new StreamReader(mspnfile))
                {
                    // error storage
                    List<string> missingcolumnlist = new List<string>(); //save the list.
                    HashSet<string> unmatchedfilecolumnlist = new HashSet<string>();
                    Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
                    int errorlinecount = 0;
                    int errordelimitercount = 0;
                    int blankvaluesadded = 0;

                    //get header.
                    string header = readfile.ReadLine();
                    string[] headersplit = header.Split(mspndel);
                    int linelength = headersplit.Length;

                    //are all columns there? 
                    List<string> filecolumn = headersplit.ToList();
                    List<int> columnindexes = new List<int>();

                    string[] expectedfilecolumns = { "Unique Customer", "MSPN", "Month", "SELLOUT UNITS" };
                    string[] newcolumns = { "ST_CUST_NBR", "MSPN_NBR", "CALENDAR_CCYYMM_NBR", "SELLOUT_UNITS" };

                    mspnmonth = mspnyear + mspnmonth;

                    foreach (var s in expectedfilecolumns)
                    {
                        if (filecolumn.Contains(s))
                        {
                            //save column index in order they appear in the expected column list - > order matches new header write order.
                            columnindexes.Add(FunctionTools.ColumnIndex(header, mspndel, s));
                        }
                        else
                        {
                            missingcolumnlist.Add(s);
                            //Console.WriteLine("ADD File missing column - {0}", s);
                        }

                        if (missingcolumnlist.Count > 0)
                        {
                            foreach (var c in filecolumn)
                            {
                                if (!expectedfilecolumns.Contains(c))
                                {
                                    unmatchedfilecolumnlist.Add(c); // columns from file that were not matched.
                                }
                            }
                        }
                    }

                    if (columnindexes.Count != expectedfilecolumns.Length)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0} - File does not contain all expected columns. Please check file headers then restart this process.", FunctionTools.GetFileNameWithoutExtension(mspnfile));
                        Console.WriteLine("************************************************************************************");
                        Console.WriteLine("Expected Columns: ");
                        foreach (var column in expectedfilecolumns)
                        {
                            Console.WriteLine(column);
                        }
                        Console.ResetColor();

                        MichelinMonthlyFileCleaner();
                    }

                    //Column being checked indexes.
                    int monthcolumn = FunctionTools.ColumnIndex(header, mspndel, "Month");

                    // read rest of file and check columns.
                    string line = string.Empty;
                    int recordcount = 0;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        recordcount++; //starts at 1.

                        string[] splitline = line.Split(mspndel);
                        int splitlength = splitline.Length;

                        while (splitlength > linelength)
                        {
                            //test line Length.
                            foreach (var value in splitline)
                            {
                                value.Trim();
                            }

                            if (line.Contains("\t\t"))
                            {
                                line = line.Replace("\t\t", "\t");
                                errordelimitercount++;
                            }
                            splitline = line.Split(mspndel);
                            splitlength = splitline.Length;
                        }

                        //test column values.
                        List<string> failreport = new List<string>();

                        // month.
                        if (splitline[monthcolumn] != mspnmonth) //test both formats.
                        {
                            failreport.Add("Month, does not match - " + splitline[monthcolumn]);
                        }

                        //outputs.
                        if (failreport.Count == 0)
                        {
                            //write to passtedtestfile, but have to match indexes first.
                            string[] newlinevaluearray = new string[platformtablecolumns.Length];

                            for (int i = 0; i <= newcolumns.Length - 1; i++)
                            {
                                foreach (var platformcolumn in platformtablecolumns)
                                {
                                    if (newcolumns[i] == platformcolumn)
                                    {
                                        //add value with index saved in columnindexes.
                                        int platformheadercolumnindex = Array.IndexOf(platformtablecolumns, platformcolumn);

                                        newlinevaluearray[platformheadercolumnindex] = splitline[columnindexes[i]];
                                    }
                                }
                            }

                            passedtestfile.WriteLine(string.Join(passedtestfiledel.ToString(), newlinevaluearray));
                        }
                        else
                        {
                            errorlinecount++; // there are errors in this line.
                            failedrecordsdict.Add(recordcount, failreport);
                        }
                    }

                    // test results
                    MichelinMonthlyFilesTestReport(mspnfile, testedoutfile, missingcolumnlist, unmatchedfilecolumnlist, failedrecordsdict, errorlinecount, errordelimitercount, blankvaluesadded);
                }
            }
        }

        public static void MonthlyKickoutFile()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine();
            Console.WriteLine("Entered required values to generate this months Kickout Out Month File");
            Console.WriteLine();
            Console.Write("Enter the new Month, first 3 letters(ex. JAN): ");
            string monthyear = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the new files Year (YYYY): ");
            string year = Console.ReadLine().Trim().ToUpper();

            Console.WriteLine("Processing..");

            string newkickoutmonthfile = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\KickOutMonth_updated.txt";
            string path = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results";

            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
            }

            // month dictionary
            Dictionary<int, string> monthdict = new Dictionary<int, string>();
            monthdict.Add(1, "JAN");
            monthdict.Add(2, "FEB");
            monthdict.Add(3, "MAR");
            monthdict.Add(4, "APR");
            monthdict.Add(5, "MAY");
            monthdict.Add(6, "JUN");
            monthdict.Add(7, "JUL");
            monthdict.Add(8, "AUG");
            monthdict.Add(9, "SEP");
            monthdict.Add(10, "OCT");
            monthdict.Add(11, "NOV");
            monthdict.Add(12, "DEC");

            // set value for day. month. use these to determine "todays" month.
            int daynumber = 1;

            int monthnumber = 0;
            foreach (var key in monthdict.Keys)
            {
                if (monthdict[key] == monthyear.ToUpper())
                {
                    monthnumber = key;
                }
            }

            int yearnumber = 0;
            if (!int.TryParse(year, out yearnumber))
            {
                Console.WriteLine("Invalid year entered.");
                Console.WriteLine();
                MichelinMonthlyFileCleaner();
            }

            // Find past months.
            List<string> monthyears = new List<string>();

            DateTime entereddate = new DateTime(yearnumber, monthnumber, daynumber);
            monthyears.Add(entereddate.ToString("MMMyyyy")); // adds month abbreviation + year.

            DateTime pastmonths = entereddate;
            // find all 12 date times.
            int d = 12;
            while (d > 1)
            {
                pastmonths = pastmonths.AddMonths(-1);
                monthyears.Add(pastmonths.ToString("MMMyyyy"));
                d--;
            }

            using (StreamWriter kickoutfile = new StreamWriter(newkickoutmonthfile))
            {
                kickoutfile.WriteLine("NAME,VALUE");

                monthyears.Reverse();

                foreach (var value in monthyears)
                {
                    List<string> linebuilder = new List<string>();
                    linebuilder.Add(value.ToUpper());
                    linebuilder.Add(value.ToUpper());
                    kickoutfile.WriteLine(string.Join(",", linebuilder.ToArray()));
                }
            }


            Console.WriteLine();
            Console.WriteLine("Done.");

        }

        public static void MichelinMonthlyFilesTestReport(string file, string testedoutfile, List<string> missingcolumnlist, HashSet<string> unmatchedfilecolumnlist, Dictionary<int, List<string>> failedrecordsdict, int errorlinecount, int errordelimitercount, int blankvaluesadded)
        {
            // error storage
            //List<string> missingcolumnlist = new List<string>(); //save the list.
            //HashSet<string> unmatchedfilecolumnlist = new HashSet<string>(); 
            //Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
            //int errorlinecount = 0;
            //int errordelimitercount = 0;
            //int blankvaluesadded = 0;

            // test results
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("{0} - Test Results:", FunctionTools.GetFileNameWithoutExtension(file));
            Console.ResetColor();

            if (errordelimitercount > 0)
            {
                // read report.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} - Delimiter errors fixed.", errordelimitercount);
                Console.ResetColor();
            }

            if (blankvaluesadded > 0)
            {
                // read report.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} - Blank values added.", blankvaluesadded);
                Console.ResetColor();
            }

            if (failedrecordsdict.Count != 0 || missingcolumnlist.Count != 0)
            {
                string failoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + FunctionTools.GetFileNameWithoutExtension(file) + "_failreport.txt";

                using (StreamWriter failreportfile = new StreamWriter(failoutfile))
                {
                    if (missingcolumnlist.Count != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Some columns are missing. Expected columns listed here - {0}", failoutfile);
                        Console.ResetColor();

                        failreportfile.WriteLine("Expected Column(s):");
                        foreach (var column in missingcolumnlist)
                        {
                            failreportfile.WriteLine(column); //outputs missing expected columns.
                        }

                        failreportfile.WriteLine(); //blank line.
                        failreportfile.WriteLine("Recieved Column(s):");
                        foreach (var column in unmatchedfilecolumnlist)
                        {
                            failreportfile.WriteLine(column); //output unmatched columns from file
                        }
                    }

                    if (failedrecordsdict.Count > 0)
                    {
                        failreportfile.WriteLine(); //blank line.
                        failreportfile.WriteLine("Record Number|FailedColumns:");
                        char faildel = '|';
                        foreach (var key in failedrecordsdict.Keys)
                        {
                            List<string> linebuilder = new List<string>();
                            linebuilder.Add(key.ToString());
                            foreach (var s in failedrecordsdict[key])
                            {
                                linebuilder.Add(s);
                            }
                            failreportfile.WriteLine(string.Join(faildel.ToString(), linebuilder.ToArray()));
                        }

                        // failed error message
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0} - records failed tests.", errorlinecount);
                        Console.WriteLine("Bad records placed in - {0}", failoutfile);
                        Console.ResetColor();

                        Console.WriteLine();
                        Console.WriteLine("Cleaned records placed in - {0}", testedoutfile);
                        Console.WriteLine();
                    }
                }
            }
            else
            {
                //passed no error message
                Console.WriteLine();
                Console.WriteLine("File Passed testing, cleaned records placed in - {0}", testedoutfile);
                Console.WriteLine();
            }
        }

        // Michelin Monthly File Testing Canada
        public static void MichelinCanadaTestMontlyFileFormats()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine();
            Console.WriteLine("This is to test 3 of the Monthly CANADA files for Michelin.");
            Console.WriteLine();
            Console.WriteLine("Please save all Michelin Monthly files to a folder on your DESKTOP.");
            Console.WriteLine("Please REMOVE ALL SPACES in the FILE NAMES and FILE PATHS.");
            Console.WriteLine("Please verify that ALL files are TAB DELIMITED.");
            Console.ResetColor();

            string path = FunctionTools.GetDesktopDirectory() + @"\michelin_canada_results\";

            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
            }

            string choice = string.Empty;
            while (choice != "exit" || choice != "Exit")
            {
                Console.WriteLine();
                Console.WriteLine("Enter test number you would like to run: ");
                Console.WriteLine("{0,5}{1,-10}", "1. ", "Test Gap Price File.");
                Console.WriteLine("{0,5}{1,-10}", "2. ", "Test and Combine GAP Files");
                //Console.WriteLine("{0,5}{1,-10}", "3. ", "Test MSPN File.");
                Console.WriteLine();
                Console.WriteLine("\"exit\" - Closes the program.");
                Console.WriteLine();
                Console.Write("Your choice: ");

                choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        MichelinCanadaTestMonthlyFilesGAPPrice();
                        break;

                    case "2":
                        MichelinCanadaTestAndCombineGapFiles();
                        break;

                    //case "3":
                    //   MichelinCanadaTestMonthlyFilesMSPN();
                    //   break;

                    case "exit":
                        Environment.Exit(0);
                        break;
                }
            }
        }

        public static void MichelinCanadaTestMonthlyFilesGAPPrice()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("Please enter the GAP Price CA TCAR file.");
            string file = FunctionTools.GetAFile();
            string filename = FunctionTools.GetFileNameWithoutExtension(file);
            char del = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month number (ex. 01): ");
            string month = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string year = Console.ReadLine().Trim().ToUpper();

            month = year + month;

            string testedoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_canada_results" + "\\" + filename + "_" + month + "_good_columns.txt";

            // error storage
            List<string> missingcolumnlist = new List<string>(); //save the list.
            HashSet<string> unmatchedfilecolumnlist = new HashSet<string>();
            Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
            int errorlinecount = 0;
            int errordelimitercount = 0;
            int blankvaluesadded = 0;

            //colum lists... they are in order of matching file -> table.
            string[] expectedfilecolumns = { "Bill To Number", "MSPN", "Month", "CA AVG INVC PRICE AMT", "CA AVG NET NET PRICE AMT" };
            string[] newcolumns = expectedfilecolumns; //columns match for now.


            using (StreamReader readfile = new StreamReader(file))
            {
                using (StreamWriter passedtestfile = new StreamWriter(testedoutfile))
                {

                    //get header.
                    string header = readfile.ReadLine();
                    string[] headersplit = header.Split(del);
                    int linelength = headersplit.Length;

                    //are all columns there? 
                    List<string> filecolumn = headersplit.ToList();
                    List<int> columnindexes = new List<int> { };

                    foreach (var s in expectedfilecolumns)
                    {
                        if (filecolumn.Contains(s))
                        {
                            //save column index in order they appear in the expected column list - > order matches new header write order.
                            columnindexes.Add(FunctionTools.ColumnIndex(header, del, s));
                        }
                        else
                        {
                            missingcolumnlist.Add(s);
                            //Console.WriteLine("ADD File missing column - {0}", s);
                        }

                        if (missingcolumnlist.Count > 0)
                        {
                            foreach (var c in filecolumn)
                            {
                                if (!expectedfilecolumns.Contains(c))
                                {
                                    unmatchedfilecolumnlist.Add(c); // columns from file that were not matched.
                                }
                            }
                        }
                    }

                    //Column indexes.
                    int monthcolumn = FunctionTools.ColumnIndex(header, del, "Month");

                    //numbertests.
                    int number1 = FunctionTools.ColumnIndex(header, del, "CA AVG INVC PRICE AMT".ToUpper());
                    int number2 = FunctionTools.ColumnIndex(header, del, "CA AVG NET NET PRICE AMT".ToUpper());
                    List<int> numbercolumnslist = new List<int> { number1, number2 };

                    //write new header -> with new column headers that match the table to be reloaded.
                    string newheader = string.Join(del.ToString(), newcolumns);
                    passedtestfile.WriteLine(newheader);

                    // read rest of file and check columns.
                    string line = string.Empty;
                    int recordcount = 0;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        //txtq replace
                        line = line.Replace("\"", "");

                        recordcount++; //starts at 1.

                        string[] splitline = line.Split(del);
                        int splitlength = splitline.Length;

                        while (splitlength > linelength)
                        {
                            //test line Length.
                            foreach (var value in splitline)
                            {
                                value.Trim();
                            }

                            if (line.Contains("\t\t"))
                            {
                                line = line.Replace("\t\t", "\t");
                                errordelimitercount++;
                            }
                            splitline = line.Split(del);
                            splitlength = splitline.Length;
                        }

                        //test column values.
                        List<string> failreport = new List<string>();

                        //number columns
                        foreach (var index in numbercolumnslist)
                        {
                            splitline[index] = splitline[index].Replace(",", string.Empty);

                            //negatives test
                            if (splitline[index].Contains(')') || splitline[index].Contains('('))
                            {
                                splitline[index] = splitline[index].Replace(")", string.Empty);
                                splitline[index] = splitline[index].Replace("(", string.Empty);
                                splitline[index] = "-" + splitline[index];
                            }

                            decimal number = 0;
                            if (!string.IsNullOrWhiteSpace(splitline[index]))
                            {
                                if (!decimal.TryParse(splitline[index], out number))
                                {
                                    failreport.Add(newcolumns[index] + ", not a number - " + splitline[index]);
                                }
                            }
                        }

                        // month.
                        if (splitline[monthcolumn] != month)
                        {
                            failreport.Add("Month, does not match - " + splitline[monthcolumn]);
                        }


                        //outputs.
                        if (failreport.Count == 0)
                        {
                            List<string> newlinebuilder = new List<string>();
                            foreach (var val in columnindexes)
                            {
                                newlinebuilder.Add(splitline[val]);
                            }

                            if (splitlength < linelength) //if the line is not long enough. we just add the value.
                            {
                                while (splitlength < linelength)
                                {
                                    newlinebuilder.Add(string.Empty);

                                    //reassign array values
                                    splitline = newlinebuilder.ToArray();
                                    blankvaluesadded++;
                                    splitlength = splitline.Length;
                                }
                            }

                            passedtestfile.WriteLine(string.Join(del.ToString(), newlinebuilder.ToArray()));
                        }
                        else
                        {
                            errorlinecount++; // there are errors in this line.
                            failedrecordsdict.Add(recordcount, failreport);
                        }
                    }
                    // test results
                    MichelinMonthlyFilesTestReport(file, testedoutfile, missingcolumnlist, unmatchedfilecolumnlist, failedrecordsdict, errorlinecount, errordelimitercount, blankvaluesadded);
                }
            }
        }

        public static void MichelinCanadaTestAndCombineGapFiles()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("Please enter the MSPN ST AAD CA OMA For Zone LAM file.");
            string mspnfile = FunctionTools.GetAFile();

            char mspndel = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month number (ex. 01): ");
            string mspnmonth = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string mspnyear = Console.ReadLine().Trim().ToUpper();

            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("Please enter the GAP CA TCAR PS Extract file.");
            string gapfile = FunctionTools.GetAFile();

            char gapdel = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month number (ex. 01): ");
            string gapmonth = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string gapyear = Console.ReadLine().Trim().ToUpper();

            //new file
            string testedoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_canada_results" + "\\" + "CanadaGAPFILE_" + gapmonth + "_good_columns.txt";
            char passedtestfiledel = '|';

            // platform table column order. output file will have these columns.
            //string[] platformtablecolumns = { "ST_CUST_NBR", "MSPN_NBR", "CALENDAR_CCYYMM_NBR", "NET_SALES_UNITS", "SELLOUT_UNITS", "NET_INVC_UNITS", "SITE_TYPE", "BIB_X_UNITS" };
            string[] platformtablecolumns = { "ST_CUST_NBR", "MSPN_NBR", "CALENDAR_CCYYMM_NBR", "NET_SALES_UNITS", "SELLOUT_UNITS", "NET_INVC_UNITS", "BIB_X_UNITS" }; // removed Site_Type. does not need to be here

            using (StreamWriter passedtestfile = new StreamWriter(testedoutfile))
            {
                //write header
                passedtestfile.WriteLine(string.Join(passedtestfiledel.ToString(), platformtablecolumns));

                //mspn file
                using (StreamReader readfile = new StreamReader(mspnfile))
                {
                    // error storage
                    List<string> missingcolumnlist = new List<string>(); //save the list.
                    HashSet<string> unmatchedfilecolumnlist = new HashSet<string>();
                    Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
                    int errorlinecount = 0;
                    int errordelimitercount = 0;
                    int blankvaluesadded = 0;

                    //get header.
                    string header = readfile.ReadLine();
                    string[] headersplit = header.Split(mspndel);
                    int linelength = headersplit.Length;

                    //are all columns there? 
                    List<string> filecolumn = headersplit.ToList();
                    List<int> columnindexes = new List<int>();

                    //colum lists... they are in order of matching file -> table.
                    string[] expectedfilecolumns = { "Unique Customer", "MSPN", "Month", "SELLOUT UNITS" };
                    string[] newcolumns = { "ST_CUST_NBR", "MSPN_NBR", "CALENDAR_CCYYMM_NBR", "SELLOUT_UNITS" }; //columns match for now.

                    //yyyyMM format
                    mspnmonth = mspnyear + mspnmonth;

                    foreach (var s in expectedfilecolumns)
                    {
                        if (filecolumn.Contains(s))
                        {
                            //save column index in order they appear in the expected column list - > order matches new header write order.
                            columnindexes.Add(FunctionTools.ColumnIndex(header, mspndel, s));
                        }
                        else
                        {
                            missingcolumnlist.Add(s);
                            //Console.WriteLine("ADD File missing column - {0}", s);
                        }

                        if (missingcolumnlist.Count > 0)
                        {
                            foreach (var c in filecolumn)
                            {
                                if (!expectedfilecolumns.Contains(c))
                                {
                                    unmatchedfilecolumnlist.Add(c); // columns from file that were not matched.
                                }
                            }
                        }
                    }

                    if (columnindexes.Count != expectedfilecolumns.Length)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0} - File does not contain all expected columns. Please check file headers then restart this process.", FunctionTools.GetFileNameWithoutExtension(mspnfile));
                        Console.WriteLine("************************************************************************************");
                        Console.ResetColor();

                        MichelinMonthlyFileCleaner();
                    }

                    //Column being checked indexes.
                    int monthcolumn = FunctionTools.ColumnIndex(header, mspndel, "Month");

                    //numbertests.
                    int selloutunitscolumn = FunctionTools.ColumnIndex(header, mspndel, "SELLOUT UNITS".ToUpper());
                    List<int> numbercolumnslist = new List<int> { selloutunitscolumn };

                    // read rest of file and check columns.
                    string line = string.Empty;
                    int recordcount = 0;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        //txtq replace
                        line = line.Replace("\"", "");

                        recordcount++; //starts at 1.

                        string[] splitline = line.Split(mspndel);
                        int splitlength = splitline.Length;

                        while (splitlength > linelength)
                        {
                            //test line Length.
                            foreach (var value in splitline)
                            {
                                value.Trim();
                            }

                            if (line.Contains("\t\t"))
                            {
                                line = line.Replace("\t\t", "\t");
                                errordelimitercount++;
                            }
                            splitline = line.Split(mspndel);
                            splitlength = splitline.Length;
                        }

                        //test column values.
                        List<string> failreport = new List<string>();

                        foreach (var index in numbercolumnslist)
                        {
                            //remove commas.
                            splitline[index] = splitline[index].Replace(",", string.Empty);

                            //negatives test
                            if (splitline[index].Contains(')') || splitline[index].Contains('('))
                            {
                                splitline[index] = splitline[index].Replace(")", string.Empty);
                                splitline[index] = splitline[index].Replace("(", string.Empty);
                                splitline[index] = "-" + splitline[index];
                            }

                            int number = 0;
                            if (!string.IsNullOrWhiteSpace(splitline[index]))
                            {
                                if (!int.TryParse(splitline[index].Replace(",", string.Empty), out number))
                                {
                                    failreport.Add(newcolumns[index] + ", not a number - " + splitline[index]);
                                }
                            }
                        }

                        // month.
                        if (splitline[monthcolumn] != mspnmonth) //test both formats.
                        {
                            failreport.Add("Month, does not match - " + splitline[monthcolumn]);
                        }

                        //outputs. OLD
                        //if (failreport.Count == 0)
                        //{
                        //   //write to passtedtestfile, but have to match indexes first.
                        //   List<string> newlinebuilder = new List<string>();

                        //   for (int i = 0; i <= newcolumns.Length - 1; i++)
                        //   {
                        //      foreach (var platformheader in platformtablecolumns)
                        //      {
                        //         if (newcolumns[i] == platformheader)
                        //         {
                        //            newlinebuilder.Add(splitline[columnindexes[i]]); //add value with index saved in columnindexes.
                        //         }
                        //      }
                        //   }

                        //   if (splitlength < platformtablecolumns.Length) //if the line is not long enough. we just add the value.
                        //   {
                        //      while (splitlength < platformtablecolumns.Length)
                        //      {
                        //         newlinebuilder.Add(string.Empty);

                        //         //reassign array values
                        //         splitline = newlinebuilder.ToArray();
                        //         blankvaluesadded++;
                        //         splitlength = splitline.Length;
                        //      }
                        //   }

                        //   passedtestfile.WriteLine(string.Join(passedtestfiledel.ToString(), newlinebuilder.ToArray()));
                        //}
                        //else
                        //{
                        //   errorlinecount++; // there are errors in this line.
                        //   failedrecordsdict.Add(recordcount, failreport);
                        //}

                        //outputs.
                        if (failreport.Count == 0)
                        {
                            //write to passtedtestfile, but have to match indexes first.
                            string[] newlinevaluearray = new string[platformtablecolumns.Length];

                            for (int i = 0; i <= newcolumns.Length - 1; i++)
                            {
                                foreach (var platformcolumn in platformtablecolumns)
                                {
                                    if (newcolumns[i] == platformcolumn)
                                    {
                                        //add value with index saved in columnindexes.
                                        int platformheadercolumnindex = Array.IndexOf(platformtablecolumns, platformcolumn);

                                        newlinevaluearray[platformheadercolumnindex] = splitline[columnindexes[i]];
                                    }
                                }
                            }

                            passedtestfile.WriteLine(string.Join(passedtestfiledel.ToString(), newlinevaluearray));
                        }
                        else
                        {
                            errorlinecount++; // there are errors in this line.
                            failedrecordsdict.Add(recordcount, failreport);
                        }

                    }
                    // test results
                    MichelinMonthlyFilesTestReport(mspnfile, testedoutfile, missingcolumnlist, unmatchedfilecolumnlist, failedrecordsdict, errorlinecount, errordelimitercount, blankvaluesadded);
                }

                // gap file
                using (StreamReader readfile = new StreamReader(gapfile))
                {
                    // error storage
                    List<string> missingcolumnlist = new List<string>(); //save the list.
                    HashSet<string> unmatchedfilecolumnlist = new HashSet<string>();
                    Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
                    int errorlinecount = 0;
                    int errordelimitercount = 0;
                    int blankvaluesadded = 0;

                    //get header.
                    string header = readfile.ReadLine();
                    string[] headersplit = header.Split(gapdel);
                    int linelength = headersplit.Length;

                    //are all columns there? 
                    List<string> filecolumn = headersplit.ToList();
                    List<int> columnindexes = new List<int>();

                    string[] expectedfilecolumns = { "Ship To Number", "MSPN", "Month", "NET SLS UNITS w NTW Bibx", "SELLOUT UNITS w/o BIB EXP for NTW", "NET INVC UNITS", "BIB EXPRESS TS UNITS Plus" };
                    string[] newcolumns = { "ST_CUST_NBR", "MSPN_NBR", "CALENDAR_CCYYMM_NBR", "NET_SALES_UNITS", "SELLOUT_UNITS", "NET_INVC_UNITS", "BIB_X_UNITS" }; //columns match for now.

                    //yyyyMM
                    gapmonth = gapyear + gapmonth;

                    foreach (var s in expectedfilecolumns)
                    {
                        if (filecolumn.Contains(s))
                        {
                            //save column index in order they appear in the expected column list - > order matches new header write order.
                            columnindexes.Add(FunctionTools.ColumnIndex(header, gapdel, s));
                        }
                        else
                        {
                            missingcolumnlist.Add(s);
                            //Console.WriteLine("ADD File missing column - {0}", s);
                        }

                        if (missingcolumnlist.Count > 0)
                        {
                            foreach (var c in filecolumn)
                            {
                                if (!expectedfilecolumns.Contains(c))
                                {
                                    unmatchedfilecolumnlist.Add(c); // columns from file that were not matched.
                                }
                            }
                        }
                    }

                    if (columnindexes.Count != expectedfilecolumns.Length)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0} - File does not contain all expected columns. Please check file headers then restart this process.", FunctionTools.GetFileNameWithoutExtension(gapfile));
                        Console.WriteLine("************************************************************************************");

                        Console.WriteLine("Expected Columns: ");
                        foreach (var column in expectedfilecolumns)
                        {
                            Console.WriteLine(column);
                        }
                        Console.ResetColor();

                        MichelinMonthlyFileCleaner();
                    }

                    //Column being checked indexes.
                    int monthcolumn = FunctionTools.ColumnIndex(header, gapdel, "Month");

                    //numbertests.
                    int number1 = FunctionTools.ColumnIndex(header, gapdel, "NET SLS UNITS w NTW Bibx".ToUpper());
                    int number2 = FunctionTools.ColumnIndex(header, gapdel, "SELLOUT UNITS w/o BIB EXP for NTW".ToUpper());
                    int number3 = FunctionTools.ColumnIndex(header, gapdel, "NET INVC UNITS".ToUpper());
                    int number4 = FunctionTools.ColumnIndex(header, gapdel, "BIB EXPRESS TS UNITS Plus".ToUpper());
                    List<int> numbercolumnslist = new List<int> { number1, number2, number3, number4 };

                    // read rest of file and check columns.
                    string line = string.Empty;
                    int recordcount = 0;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        recordcount++; //starts at 1.

                        string[] splitline = line.Split(gapdel);
                        int splitlength = splitline.Length;

                        while (splitlength > linelength)
                        {
                            //test line Length.
                            foreach (var value in splitline)
                            {
                                value.Trim();
                            }

                            if (line.Contains("\t\t"))
                            {
                                line = line.Replace("\t\t", "\t");
                                errordelimitercount++;
                            }
                            splitline = line.Split(gapdel);
                            splitlength = splitline.Length;
                        }

                        //test column values.
                        List<string> failreport = new List<string>();

                        foreach (var index in numbercolumnslist)
                        {
                            //remove commas.
                            splitline[index] = splitline[index].Replace(",", string.Empty);

                            //negatives test
                            if (splitline[index].Contains(')') || splitline[index].Contains('('))
                            {
                                splitline[index] = splitline[index].Replace(")", string.Empty);
                                splitline[index] = splitline[index].Replace("(", string.Empty);
                                splitline[index] = "-" + splitline[index];
                            }

                            int number = 0;
                            if (!string.IsNullOrWhiteSpace(splitline[index]))
                            {
                                if (!int.TryParse(splitline[index].Replace(",", string.Empty), out number))
                                {
                                    failreport.Add(newcolumns[index] + ", not a number - " + splitline[index]);
                                }
                            }
                        }

                        // month.
                        if (splitline[monthcolumn] != gapmonth) //test both formats.
                        {
                            failreport.Add("Month, does not match - " + splitline[monthcolumn]);
                        }

                        //outputs. OLD
                        //if (failreport.Count == 0)
                        //{
                        //   //write to passtedtestfile, but have to match indexes first.
                        //   List<string> newlinebuilder = new List<string>();

                        //   for (int i = 0; i <= newcolumns.Length - 1; i++)
                        //   {
                        //      foreach (var platformheader in platformtablecolumns)
                        //      {
                        //         if (newcolumns[i] == platformheader)
                        //         {
                        //            newlinebuilder.Add(splitline[columnindexes[i]]); //add value with index saved in columnindexes.
                        //         }
                        //      }
                        //   }

                        //   if (splitlength < platformtablecolumns.Length) //if the line is not long enough. we just add the value.
                        //   {
                        //      while (splitlength < platformtablecolumns.Length)
                        //      {
                        //         newlinebuilder.Add(string.Empty);

                        //         //reassign array values
                        //         splitline = newlinebuilder.ToArray();
                        //         blankvaluesadded++;
                        //         splitlength = splitline.Length;
                        //      }
                        //   }

                        //   passedtestfile.WriteLine(string.Join(passedtestfiledel.ToString(), newlinebuilder.ToArray()));
                        //}
                        //else
                        //{
                        //   errorlinecount++; // there are errors in this line.
                        //   failedrecordsdict.Add(recordcount, failreport);
                        //}

                        //outputs.
                        if (failreport.Count == 0)
                        {
                            //write to passtedtestfile, but have to match indexes first.
                            string[] newlinevaluearray = new string[platformtablecolumns.Length];

                            for (int i = 0; i <= newcolumns.Length - 1; i++)
                            {
                                foreach (var platformcolumn in platformtablecolumns)
                                {
                                    if (newcolumns[i] == platformcolumn)
                                    {
                                        //add value with index saved in columnindexes.
                                        int platformheadercolumnindex = Array.IndexOf(platformtablecolumns, platformcolumn);

                                        newlinevaluearray[platformheadercolumnindex] = splitline[columnindexes[i]];
                                    }
                                }
                            }

                            passedtestfile.WriteLine(string.Join(passedtestfiledel.ToString(), newlinevaluearray));
                        }
                        else
                        {
                            errorlinecount++; // there are errors in this line.
                            failedrecordsdict.Add(recordcount, failreport);
                        }
                    }

                    // test results
                    MichelinMonthlyFilesTestReport(gapfile, testedoutfile, missingcolumnlist, unmatchedfilecolumnlist, failedrecordsdict, errorlinecount, errordelimitercount, blankvaluesadded);
                }
            }
        }


        //old versions.
        public static void MichelinDealerTireGapMonthly()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("Please enter the US Dealer Tire GAP Batch file.");
            string usdtgapfile = FunctionTools.GetAFile();
            string usdtgapfilename = FunctionTools.GetFileNameWithoutExtension(usdtgapfile);
            string usdtgapoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + usdtgapfilename + "_good_columns.txt";
            string usdtgapoutfile2 = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + usdtgapfilename + "_bad_columns.txt";

            char usdtgapdel = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month first 3 letters(ex. JAN): ");
            string usdtgapoldmonth = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the file Month number (ex. 01): ");
            string usdtgapmonth = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string usdtgapyear = Console.ReadLine().Trim().ToUpper();

            //new month, yyyymm all numbers.
            usdtgapmonth = usdtgapyear + usdtgapmonth;
            //old MMMyyyy
            usdtgapoldmonth = usdtgapoldmonth + usdtgapyear;

            using (StreamReader readfile = new StreamReader(usdtgapfile))
            {
                using (StreamWriter outfile1 = new StreamWriter(usdtgapoutfile))
                {
                    using (StreamWriter outfile2 = new StreamWriter(usdtgapoutfile2))
                    {
                        //column definitions.
                        string[] expectedcolumnnames = { "Indirect Customer", "MSPN", "Month", "Year", "SELLOUT UNITS" };
                        string[] newcolumnnames = { "ST_CUST_NBR", "MSPN_NBR", "CALENDAR_CCYYMM_NBR", "SELLOUT_UNITS" };

                        //get header.
                        string usdtgapheader = readfile.ReadLine();
                        string[] columns = usdtgapheader.Split(usdtgapdel);
                        int usdtgaplength = columns.Length;

                        //are all columns there? 
                        List<string> columnchecker = new List<string> { };
                        List<int> columnindexes = new List<int> { };

                        foreach (var s in columns)
                        {
                            columnchecker.Add(s);
                        }

                        bool columnsmissing = false;
                        foreach (var s in expectedcolumnnames)
                        {
                            if (!columnchecker.Contains(s))
                            {
                                columnsmissing = true;
                                Console.WriteLine("US Dealer Tire GAP File missing column - {0}", s);
                            }
                            else
                            {
                                //save column index in order they appear in the expected column list - > order matches new header write order.
                                columnindexes.Add(FunctionTools.ColumnIndex(usdtgapheader, usdtgapdel, s));
                            }
                        }

                        if (columnsmissing == true)
                        {
                            List<string> outcolumns = expectedcolumnnames.ToList();
                            foreach (var s in outcolumns)
                            {
                                outfile2.WriteLine(s);
                            }

                            Console.WriteLine("Some columns are missing, please check the File, program will now close.");
                            Console.WriteLine("Expected columns listed here - {0}", outfile2);

                        }

                        //write new header -> with new column headers that match the table to be reloaded.
                        string newheader = string.Join(usdtgapdel.ToString(), newcolumnnames);
                        outfile1.WriteLine(newheader);

                        //write to error file
                        outfile2.WriteLine("fail_report\t" + newheader);

                        //Column indexes.
                        //int uniquecustid = ColumnIndex(usdtlamheader, usdtlamdel, "Plan Channel Member Group ID");
                        int year = FunctionTools.ColumnIndex(usdtgapheader, usdtgapdel, "Year");
                        int Month = FunctionTools.ColumnIndex(usdtgapheader, usdtgapdel, "Month");

                        string line = string.Empty;
                        int count = 0;
                        while ((line = readfile.ReadLine()) != null)
                        {
                            line = line.Replace(",", string.Empty);
                            string[] splitline = line.Split(usdtgapdel);
                            int splitlength = splitline.Length;

                            //test line Length.
                            while (splitlength != usdtgaplength)
                            {
                                //line = line.Replace("\t\t", "\t");
                                splitline = line.Split(usdtgapdel);
                                splitlength = splitline.Length;
                            }

                            //test column values.
                            //int number = 0;
                            string failreport = string.Empty;
                            // unique customer id
                            //if (!int.TryParse(splitline[uniquecustid], out number))
                            //{
                            //   failreport = failreport + "Unique Customer Id: Not a Number -> ";
                            //}

                            // year.
                            if (splitline[year] != usdtgapyear)
                            {
                                failreport = failreport + "Year: does not match " + year + " -> ";
                            }

                            // month.
                            if (splitline[Month] != usdtgapoldmonth && splitline[Month] != usdtgapmonth)
                            {
                                failreport = failreport + splitline[Month] + " does not match expected" + usdtgapoldmonth + " -> ";
                            }

                            //outputs.
                            if (failreport == string.Empty)
                            {
                                //if everything checks out. write the line in the order of new columns -> from columnindexes.
                                List<string> newlinebuilder = new List<string> { };

                                //change the month column to yyyymm.
                                splitline[Month] = usdtgapmonth;

                                //from og line. get 0,1,2,4. skip the year column. 
                                foreach (int i in columnindexes)
                                {
                                    if (i != year)
                                    {
                                        newlinebuilder.Add(splitline[i]);
                                    }
                                }

                                string newline = string.Join(usdtgapdel.ToString(), newlinebuilder.ToArray());

                                outfile1.WriteLine(newline);
                            }
                            else
                            {
                                outfile2.WriteLine(failreport + "\t" + line);
                                count++;
                            }
                        }

                        Console.WriteLine();
                        Console.WriteLine("Good records placed in - {0}", FunctionTools.GetFileNameWithoutExtension(usdtgapoutfile));
                        Console.WriteLine("{1} - Bad records placed in - {0}", FunctionTools.GetFileNameWithoutExtension(usdtgapoutfile2), count);
                        Console.WriteLine();
                    }
                }
            }
        }

        public static void TestMonthlyFilesGAPTCAR()
        {
            //added splitline[x] = splitline[x].Replace(",", string.Empty); in foreach loop of columns to check 12/10/18

            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("Please enter the GAP TCAR file.");
            string gapfile = FunctionTools.GetAFile();
            string gapfilename = FunctionTools.GetFileNameWithoutExtension(gapfile);
            string gapoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + gapfilename + "_good_columns.txt";
            string gapoutfile2 = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + gapfilename + "_bad_columns.txt";

            char gapdel = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month number (ex. 01): ");
            string gapmonth = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string gapyear = Console.ReadLine().Trim().ToUpper();

            gapmonth = gapyear + gapmonth;







            using (StreamReader readgapfile = new StreamReader(gapfile))
            {
                using (StreamWriter writegap1 = new StreamWriter(gapoutfile))
                {
                    using (StreamWriter writegap2 = new StreamWriter(gapoutfile2))
                    {
                        string[] expectedfilecolumns = { "Ship To Number", "MSPN", "Month", "NET SLS UNITS w NTW Bibx", "SELLOUT UNITS w/o BIB EXP for NTW", "NET INVC UNITS", "BIB EXPRESS TS UNITS Plus" };
                        string[] newcolumns = { "ST_CUST_NBR", "MSPN_NBR", "CALENDAR_CCYYMM_NBR", "NET_SALES_UNITS", "SELLOUT_UNITS", "NET_INVC_UNITS", "BIB_X_UNITS" };

                        //List<int> commacleaner = new List<int>{}

                        //get header.
                        string gapheader = readgapfile.ReadLine();
                        string[] columns = gapheader.Split(gapdel);
                        int gaplength = columns.Length;

                        //are all columns there? 
                        List<string> columnchecker = new List<string> { };
                        List<int> columnindexes = new List<int> { };

                        foreach (var s in columns)
                        {
                            columnchecker.Add(s);
                        }

                        bool columnsmissing = false;
                        foreach (var s in expectedfilecolumns)
                        {
                            if (!columnchecker.Contains(s))
                            {
                                columnsmissing = true;
                                Console.WriteLine("GAPTCAR File missing column - {0}", s);
                            }
                            else
                            {
                                //save column index in order they appear in the expected column list - > order matches new header write order.
                                columnindexes.Add(FunctionTools.ColumnIndex(gapheader, gapdel, s));
                            }
                        }

                        if (columnsmissing == true)
                        {
                            List<string> outcolumns = expectedfilecolumns.ToList();
                            foreach (var s in outcolumns)
                            {
                                writegap2.WriteLine(s);
                            }

                            Console.WriteLine("Some columns are missing, please check the File, program will now close.");
                            Console.WriteLine("Expected columns listed here - {0}", gapoutfile2);

                        }

                        //write new header -> with new column headers that match the table to be reloaded.
                        string newheader = string.Join(gapdel.ToString(), newcolumns);
                        writegap1.WriteLine(newheader);

                        //write to error file
                        writegap2.WriteLine("fail_report\t" + newheader);


                        //Column indexes.
                        int Month = FunctionTools.ColumnIndex(gapheader, gapdel, "Month".ToUpper());

                        //numbertests.
                        int etslsunits = FunctionTools.ColumnIndex(gapheader, gapdel, "ET SLS UNITS w NTW Bibx".ToUpper());
                        int selloutunits = FunctionTools.ColumnIndex(gapheader, gapdel, "SELLOUT UNITS w/o BIB EXP for NTW".ToUpper());
                        int netinvcunits = FunctionTools.ColumnIndex(gapheader, gapdel, "NET INVC UNITS".ToUpper());
                        int bibexpress = FunctionTools.ColumnIndex(gapheader, gapdel, "BIB EXPRESS TS UNITS".ToUpper());

                        List<int> columnslist = new List<int> { etslsunits, selloutunits, netinvcunits, bibexpress };

                        //read rest of file and check columns.
                        int count = 0;
                        int linesread = 0;
                        //char tab = '\u0009';
                        string line = string.Empty;
                        while ((line = readgapfile.ReadLine()) != null)
                        {
                            //line = line.Replace(",", string.Empty);
                            string[] splitline = line.Split(gapdel);
                            int splitlength = splitline.Length;

                            //test line Length.
                            while (splitlength != gaplength)
                            {
                                line = line.Replace("\t\t", "\t");
                                splitline = line.Split(gapdel);
                                splitlength = splitline.Length;
                            }

                            //test column values.
                            int number = 0;
                            string failreport = string.Empty;
                            //number tests.
                            foreach (var x in columnslist)
                            {
                                splitline[x] = splitline[x].Replace(",", string.Empty);
                                if (!string.IsNullOrWhiteSpace(splitline[x]))
                                {
                                    if (!int.TryParse(splitline[x], out number))
                                    {
                                        failreport = failreport + columns[x] + ": Not a Number -> ";
                                    }
                                }
                            }

                            // month.
                            if (splitline[Month] != gapmonth)
                            {
                                failreport = failreport + "Month: does not match " + gapmonth + " -> ";
                            }

                            //outputs.
                            if (failreport == string.Empty)
                            {
                                //if everything checks out. write the line in the order of new columns -> from columnindexes.
                                List<string> newlinebuilder = new List<string> { };

                                foreach (int i in columnindexes)
                                {
                                    newlinebuilder.Add(splitline[i]);
                                }

                                string newline = string.Join(gapdel.ToString(), newlinebuilder.ToArray());

                                writegap1.WriteLine(newline);
                            }
                            else
                            {
                                writegap2.WriteLine(failreport + "\t" + line);
                                count++;
                            }
                            linesread++;
                        }

                        Console.WriteLine();
                        Console.WriteLine("Good records placed in - {0}", FunctionTools.GetFileNameWithoutExtension(gapoutfile));
                        Console.WriteLine("{1} - Bad records placed in - {0}", FunctionTools.GetFileNameWithoutExtension(gapoutfile2), count);
                        Console.WriteLine();
                    }
                }
            }

        }

        public static void MichelinTestMonthlyFilesMSPN()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("Please enter the MSPN file.");
            string file = FunctionTools.GetAFile();
            string filename = FunctionTools.GetFileNameWithoutExtension(file);
            string testedoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + filename + "_good_columns.txt";


            char del = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month number (ex. 01): ");
            string month = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string year = Console.ReadLine().Trim().ToUpper();

            month = year + month;

            // error storage
            List<string> missingcolumnlist = new List<string>(); //save the list.
            HashSet<string> unmatchedfilecolumnlist = new HashSet<string>();
            Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
            int errorlinecount = 0;
            int errordelimitercount = 0;
            int blankvaluesadded = 0;

            //colum lists... they are in order of matching file -> table.
            string[] expectedfilecolumns = { "Unique Customer", "MSPN", "Month", "SELLOUT UNITS" };
            string[] newcolumns = { "ST_CUST_NBR", "MSPN_NBR", "CALENDAR_CCYYMM_NBR", "SELLOUT_UNITS" };

            using (StreamReader readfile = new StreamReader(file))
            {
                using (StreamWriter passedtestfile = new StreamWriter(testedoutfile))
                {
                    //get header.
                    string header = readfile.ReadLine();
                    string[] headersplit = header.Split(del);
                    int linelength = headersplit.Length;

                    //are all columns there? 
                    List<string> filecolumn = headersplit.ToList();
                    List<int> columnindexes = new List<int> { };

                    foreach (var s in expectedfilecolumns)
                    {
                        if (filecolumn.Contains(s))
                        {
                            //save column index in order they appear in the expected column list - > order matches new header write order.
                            columnindexes.Add(FunctionTools.ColumnIndex(header, del, s));
                        }
                        else
                        {
                            missingcolumnlist.Add(s);
                            //Console.WriteLine("ADD File missing column - {0}", s);
                        }

                        if (missingcolumnlist.Count > 0)
                        {
                            foreach (var c in filecolumn)
                            {
                                if (!expectedfilecolumns.Contains(c))
                                {
                                    unmatchedfilecolumnlist.Add(c); // columns from file that were not matched.
                                }
                            }
                        }
                    }

                    //Column being checked indexes.
                    int monthcolumn = FunctionTools.ColumnIndex(header, del, "Month".ToUpper());


                    //write new header -> with new column headers that match the table to be reloaded.
                    string newheader = string.Join(del.ToString(), newcolumns);
                    passedtestfile.WriteLine(newheader);

                    // read rest of file and check columns.
                    string line = string.Empty;
                    int recordcount = 0;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        recordcount++; //starts at 1.

                        string[] splitline = line.Split(del);
                        int splitlength = splitline.Length;

                        while (splitlength > linelength)
                        {
                            //test line Length.
                            foreach (var value in splitline)
                            {
                                value.Trim();
                            }

                            if (line.Contains("\t\t"))
                            {
                                line = line.Replace("\t\t", "\t");
                                errordelimitercount++;
                            }
                            splitline = line.Split(del);
                            splitlength = splitline.Length;
                        }

                        if (splitlength < linelength) //if the line is not long enough. we just add the value.
                        {
                            List<string> newsplitline = new List<string>();
                            foreach (var value in splitline)
                            {
                                value.Trim();
                                newsplitline.Add(value);
                            }

                            while (splitlength < linelength)
                            {
                                newsplitline.Add(string.Empty);

                                //reassign array values
                                splitline = newsplitline.ToArray();
                                blankvaluesadded++;
                                splitlength = splitline.Length;
                            }

                        }

                        //test column values.
                        List<string> failreport = new List<string>();

                        // month.
                        if (splitline[monthcolumn] != month)
                        {
                            failreport.Add("Month, does not match - " + splitline[monthcolumn]);
                        }

                        //outputs.
                        if (failreport.Count == 0)
                        {
                            List<string> newlinebuilder = new List<string>();
                            foreach (var val in columnindexes)
                            {
                                newlinebuilder.Add(splitline[val]);
                            }

                            passedtestfile.WriteLine(string.Join(del.ToString(), newlinebuilder.ToArray()));
                        }
                        else
                        {
                            errorlinecount++; // there are errors in this line.
                            failedrecordsdict.Add(recordcount, failreport);
                        }
                    }

                    // test results
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("{0} - Test Results:", FunctionTools.GetFileNameWithoutExtension(file));
                    Console.ResetColor();

                    if (errordelimitercount > 0)
                    {
                        // read report.
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0} - Delimiter errors fixed.", errordelimitercount);
                        Console.WriteLine("{0} - Blank values added.", blankvaluesadded);
                        Console.ResetColor();
                    }

                    if (failedrecordsdict.Count != 0 || missingcolumnlist.Count != 0)
                    {
                        string failoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + filename + "_failreport.txt";

                        using (StreamWriter failreportfile = new StreamWriter(failoutfile))
                        {
                            if (missingcolumnlist.Count != 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Some columns are missing. Expected columns listed here - {0}", failoutfile);
                                Console.ResetColor();

                                failreportfile.WriteLine("Expected Column(s):");
                                foreach (var column in missingcolumnlist)
                                {
                                    failreportfile.WriteLine(column); //outputs missing expected columns.
                                }

                                failreportfile.WriteLine(); //blank line.
                                failreportfile.WriteLine("Recieved Column(s):");
                                foreach (var column in unmatchedfilecolumnlist)
                                {
                                    failreportfile.WriteLine(column); //output unmatched columns from file
                                }

                            }

                            if (failedrecordsdict.Count > 0)
                            {
                                failreportfile.WriteLine(); //blank line.
                                failreportfile.WriteLine("Record Number|FailedColumns:");
                                char faildel = '|';
                                foreach (var key in failedrecordsdict.Keys)
                                {
                                    List<string> linebuilder = new List<string>();
                                    linebuilder.Add(key.ToString());
                                    foreach (var s in failedrecordsdict[key])
                                    {
                                        linebuilder.Add(s);
                                    }
                                    failreportfile.WriteLine(string.Join(faildel.ToString(), linebuilder.ToArray()));
                                }

                                // failed error message
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("{0} - records failed tests.", errorlinecount);
                                Console.WriteLine("Bad records placed in - {0}", failoutfile);
                                Console.ResetColor();

                                Console.WriteLine();
                                Console.WriteLine("Cleaned records placed in - {0}", testedoutfile);
                                Console.WriteLine();
                            }
                        }
                    }
                    else
                    {
                        //passed no error message
                        Console.WriteLine();
                        Console.WriteLine("File Passed testing, cleaned records placed in - {0}", testedoutfile);
                        Console.WriteLine();
                    }
                }
            }
        }

        public static void MichelinCanadaTestMonthlyFilesMSPN()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("Please enter the MSPN file.");
            string file = FunctionTools.GetAFile();
            string filename = FunctionTools.GetFileNameWithoutExtension(file);
            string testedoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_canada_results" + "\\" + filename + "_good_columns.txt";

            char del = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month number (ex. 01): ");
            string month = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string year = Console.ReadLine().Trim().ToUpper();

            month = year + month;

            // error storage
            List<string> missingcolumnlist = new List<string>(); //save the list.
            HashSet<string> unmatchedfilecolumnlist = new HashSet<string>();
            Dictionary<int, List<string>> failedrecordsdict = new Dictionary<int, List<string>>();
            int errorlinecount = 0;
            int errordelimitercount = 0;
            int blankvaluesadded = 0;

            //colum lists... they are in order of matching file -> table.
            string[] expectedfilecolumns = { "Unique Customer", "MSPN", "Month", "SELLOUT UNITS" };
            string[] newcolumns = { "ST_CUST_NBR", "MSPN_NBR", "CALENDAR_CCYYMM_NBR", "SELLOUT_UNITS" }; //columns match for now.

            using (StreamReader readfile = new StreamReader(file))
            {
                using (StreamWriter passedtestfile = new StreamWriter(testedoutfile))
                {
                    //get header.
                    string header = readfile.ReadLine();
                    string[] headersplit = header.Split(del);
                    int linelength = headersplit.Length;

                    //are all columns there? 
                    List<string> filecolumn = new List<string> { };
                    List<int> columnindexes = new List<int> { };

                    foreach (var s in expectedfilecolumns)
                    {
                        if (filecolumn.Contains(s))
                        {
                            //save column index in order they appear in the expected column list - > order matches new header write order.
                            columnindexes.Add(FunctionTools.ColumnIndex(header, del, s));
                        }
                        else
                        {
                            missingcolumnlist.Add(s);
                            //Console.WriteLine("ADD File missing column - {0}", s);
                        }

                        if (missingcolumnlist.Count > 0)
                        {
                            foreach (var c in filecolumn)
                            {
                                if (!expectedfilecolumns.Contains(c))
                                {
                                    unmatchedfilecolumnlist.Add(c); // columns from file that were not matched.
                                }
                            }
                        }
                    }

                    //Column being checked indexes.
                    int monthcolumn = FunctionTools.ColumnIndex(header, del, "Month");

                    //numbertests.
                    int number1 = FunctionTools.ColumnIndex(header, del, "SELLOUT UNITS".ToUpper());
                    List<int> numbercolumnslist = new List<int> { number1 };

                    //write new header -> with new column headers that match the table to be reloaded.
                    string newheader = string.Join(del.ToString(), newcolumns);
                    passedtestfile.WriteLine(newheader);

                    // read rest of file and check columns.
                    string line = string.Empty;
                    int recordcount = 0;
                    while ((line = readfile.ReadLine()) != null)
                    {
                        recordcount++; //starts at 1.

                        string[] splitline = line.Split(del);
                        int splitlength = splitline.Length;

                        while (splitlength > linelength)
                        {
                            //test line Length.
                            foreach (var value in splitline)
                            {
                                value.Trim();
                            }

                            if (line.Contains("\t\t"))
                            {
                                line = line.Replace("\t\t", "\t");
                                errordelimitercount++;
                            }
                            splitline = line.Split(del);
                            splitlength = splitline.Length;
                        }

                        if (splitlength < linelength) //if the line is not long enough. we just add the value.
                        {
                            List<string> newsplitline = new List<string>();
                            foreach (var value in splitline)
                            {
                                value.Trim();
                                newsplitline.Add(value);
                            }

                            while (splitlength < linelength)
                            {
                                newsplitline.Add(string.Empty);

                                //reassign array values
                                splitline = newsplitline.ToArray();
                                blankvaluesadded++;
                                splitlength = splitline.Length;
                            }
                        }

                        //test column values.
                        List<string> failreport = new List<string>();

                        //number columns (this one is unique to this file).
                        foreach (var index in numbercolumnslist)
                        {
                            //remove commas.
                            splitline[index] = splitline[index].Replace(",", string.Empty);

                            //negatives test
                            if (splitline[index].Contains(')') || splitline[index].Contains('('))
                            {
                                splitline[index] = splitline[index].Replace(")", string.Empty);
                                splitline[index] = splitline[index].Replace("(", string.Empty);
                                splitline[index] = "-" + splitline[index];
                            }

                            decimal number = 0;
                            if (!string.IsNullOrWhiteSpace(splitline[index]))
                            {
                                if (!decimal.TryParse(splitline[index], out number))
                                {
                                    failreport.Add(newcolumns[index] + ", not a number - " + splitline[index]);
                                }
                            }
                        }

                        // month.
                        if (splitline[monthcolumn] != month)
                        {
                            failreport.Add("Month, does not match - " + splitline[monthcolumn]);
                        }

                        //outputs.
                        if (failreport.Count == 0)
                        {
                            List<string> newlinebuilder = new List<string>();
                            foreach (var val in columnindexes)
                            {
                                newlinebuilder.Add(splitline[val]);
                            }

                            passedtestfile.WriteLine(string.Join(del.ToString(), newlinebuilder.ToArray()));
                        }
                        else
                        {
                            errorlinecount++; // there are errors in this line.
                            failedrecordsdict.Add(recordcount, failreport);
                        }
                    }

                    // test results
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("{0} - Test Results:", FunctionTools.GetFileNameWithoutExtension(file));
                    Console.ResetColor();

                    if (errordelimitercount > 0)
                    {
                        // read report.
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0} - Delimiter errors fixed.", errordelimitercount);
                        Console.WriteLine("{0} - Blank values added.", blankvaluesadded);
                        Console.ResetColor();
                    }

                    if (failedrecordsdict.Count != 0 || missingcolumnlist.Count != 0)
                    {
                        string failoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_us_results" + "\\" + filename + "_failreport.txt";

                        using (StreamWriter failreportfile = new StreamWriter(failoutfile))
                        {
                            if (missingcolumnlist.Count != 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Some columns are missing. Expected columns listed here - {0}", failoutfile);
                                Console.ResetColor();

                                failreportfile.WriteLine("Expected Column(s):");
                                foreach (var column in missingcolumnlist)
                                {
                                    failreportfile.WriteLine(column); //outputs missing expected columns.
                                }

                                failreportfile.WriteLine(); //blank line.
                                failreportfile.WriteLine("Recieved Column(s):");
                                foreach (var column in unmatchedfilecolumnlist)
                                {
                                    failreportfile.WriteLine(column); //output unmatched columns from file
                                }

                            }

                            if (failedrecordsdict.Count > 0)
                            {
                                failreportfile.WriteLine(); //blank line.
                                failreportfile.WriteLine("Record Number|FailedColumns:");
                                char faildel = '|';
                                foreach (var key in failedrecordsdict.Keys)
                                {
                                    List<string> linebuilder = new List<string>();
                                    linebuilder.Add(key.ToString());
                                    foreach (var s in failedrecordsdict[key])
                                    {
                                        linebuilder.Add(s);
                                    }
                                    failreportfile.WriteLine(string.Join(faildel.ToString(), linebuilder.ToArray()));
                                }

                                // failed error message
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("{0} - records failed tests.", errorlinecount);
                                Console.WriteLine("Bad records placed in - {0}", failoutfile);
                                Console.ResetColor();

                                Console.WriteLine();
                                Console.WriteLine("Cleaned records placed in - {0}", testedoutfile);
                                Console.WriteLine();
                            }
                        }
                    }
                    else
                    {
                        //passed no error message
                        Console.WriteLine();
                        Console.WriteLine("File Passed testing, cleaned records placed in - {0}", testedoutfile);
                        Console.WriteLine();
                    }
                }
            }

        }

        public static void MichelinCanadaTestMonthlyFilesGAPTCAR()
        {
            Console.WriteLine();
            Console.WriteLine("************************************************************************************");
            Console.WriteLine("Please enter the Gap TCAR file.");
            string gapfile = FunctionTools.GetAFile();
            string gapfilename = FunctionTools.GetFileNameWithoutExtension(gapfile);
            string gapoutfile = FunctionTools.GetDesktopDirectory() + @"\michelin_canada_results" + "\\" + gapfilename + "_good_columns.txt";
            string gapoutfile2 = FunctionTools.GetDesktopDirectory() + @"\michelin_canada_results" + "\\" + gapfilename + "_bad_columns.txt";

            char gapdel = FunctionTools.GetDelimiter();
            Console.Write("Enter the file Month number (ex. 01): ");
            string gapmonth = Console.ReadLine().Trim().ToUpper();
            Console.Write("Enter the files Year (YYYY): ");
            string gapyear = Console.ReadLine().Trim().ToUpper();

            gapmonth = gapyear + gapmonth;

            using (StreamReader readfile = new StreamReader(gapfile))
            {
                using (StreamWriter gapgoodfile = new StreamWriter(gapoutfile))
                {
                    using (StreamWriter gapbadfile = new StreamWriter(gapoutfile2))
                    {

                        string[] expectedcolumns = { "Ship To Number", "MSPN", "Month", "NET SLS UNITS w NTW Bibx", "SELLOUT UNITS w/o BIB EXP for NTW", "NET INVC UNITS", "BIB EXPRESS TS UNITS Plus" };
                        string[] newcolumnnames = { "ST_CUST_NBR", "MSPN_NBR", "CALENDAR_CCYYMM_NBR", "NET_SALES_UNITS", "SELLOUT_UNITS", "NET_INVC_UNITS", "BIB_X_UNITS" }; //columns match for now.

                        //get header.
                        string gapheader = readfile.ReadLine();
                        string[] columns = gapheader.Split(gapdel);
                        int gaplength = columns.Length;

                        //are all columns there? 
                        List<string> columnchecker = new List<string> { };
                        List<int> columnindexes = new List<int> { };

                        foreach (var s in columns)
                        {
                            columnchecker.Add(s);
                        }

                        bool columnsmissing = false;
                        foreach (var s in expectedcolumns)
                        {
                            if (!columnchecker.Contains(s))
                            {
                                columnsmissing = true;
                                Console.WriteLine("CA GAP TCAR File missing column - {0}", s);
                            }
                            else
                            {
                                //save column index in order they appear in the expected column list - > order matches new header write order.
                                columnindexes.Add(FunctionTools.ColumnIndex(gapheader, gapdel, s));
                            }
                        }

                        if (columnsmissing == true)
                        {
                            List<string> outcolumns = expectedcolumns.ToList();
                            foreach (var s in outcolumns)
                            {
                                gapbadfile.WriteLine(s);
                            }

                            Console.WriteLine("Some columns are missing, please check the File, program will now close.");
                            Console.WriteLine("Expected columns listed here - {0}", gapbadfile);
                        }

                        //write new header -> with new column headers that match the table to be reloaded.
                        string newheader = string.Join(gapdel.ToString(), newcolumnnames);
                        gapgoodfile.WriteLine(newheader);

                        //write to error file
                        gapbadfile.WriteLine("fail_report\t" + newheader);

                        //Column indexes.
                        int month = FunctionTools.ColumnIndex(gapheader, gapdel, "Month");

                        //numbertests.
                        int number1 = FunctionTools.ColumnIndex(gapheader, gapdel, "NET SLS UNITS w NTW Bibx".ToUpper());
                        int number2 = FunctionTools.ColumnIndex(gapheader, gapdel, "SELLOUT UNITS w/o BIB EXP for NTW".ToUpper());
                        int number3 = FunctionTools.ColumnIndex(gapheader, gapdel, "NET INVC UNITS".ToUpper());
                        int number4 = FunctionTools.ColumnIndex(gapheader, gapdel, "BIB EXPRESS TS UNITS Plus".ToUpper());
                        List<int> columnslist = new List<int> { number1, number2, number3, number4 };

                        string line = string.Empty;
                        int count = 0;
                        while ((line = readfile.ReadLine()) != null)
                        {
                            //line = line.Replace(",", string.Empty);
                            string[] splitline = line.Split(gapdel);
                            int splitlength = splitline.Length;

                            //test line Length.
                            while (splitlength != gaplength)
                            {
                                line = line.Replace("\t\t", "\t");
                                splitline = line.Split(gapdel);
                                splitlength = splitline.Length;
                            }

                            string failreport = string.Empty;
                            decimal number = 0;
                            // year.
                            //if (splitline[year] != usdtlamyear)
                            //{
                            //   failreport = failreport + "Year: does not match " + year + " -> ";
                            //}

                            // month.
                            if (splitline[month] != gapmonth)
                            {
                                failreport = failreport + "Month: does not match " + gapmonth + " -> ";
                            }

                            //number test
                            foreach (var x in columnslist)
                            {
                                splitline[x] = splitline[x].Replace(",", string.Empty);

                                //negatives test
                                if (splitline[x].Contains(')') || splitline[x].Contains('('))
                                {
                                    splitline[x] = splitline[x].Replace(")", string.Empty);
                                    splitline[x] = splitline[x].Replace("(", string.Empty);
                                    splitline[x] = "-" + splitline[x];
                                }

                                if (!string.IsNullOrWhiteSpace(splitline[x]))
                                {
                                    if (!decimal.TryParse(splitline[x], out number))
                                    {
                                        failreport = failreport + columns[x] + ": Not a Number -> ";
                                    }
                                }
                            }

                            //outputs.
                            if (failreport == string.Empty)
                            {
                                //if everything checks out. write the line in the order of new columns -> from columnindexes.
                                List<string> newlinebuilder = new List<string> { };

                                foreach (int i in columnindexes)
                                {
                                    newlinebuilder.Add(splitline[i]);
                                }

                                string newline = string.Join(gapdel.ToString(), newlinebuilder.ToArray());

                                gapgoodfile.WriteLine(newline);
                            }
                            else
                            {
                                gapbadfile.WriteLine(failreport + "\t" + line);
                                count++;
                            }
                        }

                        Console.WriteLine();
                        Console.WriteLine("Good records placed in - {0}", FunctionTools.GetFileNameWithoutExtension(gapoutfile));
                        Console.WriteLine("{1} - Bad records placed in - {0}", FunctionTools.GetFileNameWithoutExtension(gapoutfile2), count);
                        Console.WriteLine();

                    }
                }
            }
        }

    }
}
