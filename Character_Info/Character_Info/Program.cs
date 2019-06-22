/*
  Copyright (c) 2017 Karl Olsen @ droodicus at http://gmail.com 
  Please see "license.txt" in the top-most folder for more information.
 
WRITTEN BY KARL OLSEN 
droodicus@gmail.com
twitter.com/Droodicus
github.com/drood1
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Character_Info
{
    public class Talent
    {
        public int tier;
        public string name;
        public string description;
        public string icon;

        public Talent()
        {
            name = "";
        }

        public Talent(int t, string n, string d, string i) 
        {
            tier = t;
            name = n;
            description = d;
            icon = i;
        }

        public void printInfo()
        {
            Console.WriteLine("Tier: {0}", tier);
            Console.WriteLine("Name: {0}", name);
            Console.WriteLine("Description: {0}", description);
            //Console.WriteLine("Icon: {0}\n", icon);
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            //url-formation data
            string char_name;
            string server;
            string temp_server = "";
            string base_item_url = "https://us.api.blizzard.com/wow/character/";
            //string locale = "?locale=en_US";
            string talent_url = "?fields=talents&locale=en_US";
            string api_key = "&access_token=USEcsbHXPXbM3L1I3fqCmJSRnHU7pxWxEX";
            string full_url;
            string full_information;

            //talent/spec information storage
            int num_specs = 0; //number of possible specs (2 for Demon Hunters, 4 for Druids, 3 for everyone else)
            List<Talent> talents_first = new List<Talent>();
            List<Talent> talents_second = new List<Talent>();
            List<Talent> talents_third = new List<Talent>();
            List<Talent> talents_fourth = new List<Talent>();
            //markers for when to move on to the "next" spec's talents
            bool primary_talents_done = false;
            bool second_talents_done = false;
            bool third_talents_done = false;
            //spec names
            string primary_spec = "";
            string secondary_spec = "";
            string third_spec = "";
            string fourth_spec = "";
            //temp storage variables for new talent class objects
            int temp_tier;
            string temp_name = "";
            string temp_desc = "";
            string temp_icon = "";

            //characters to "ignore" while reading the item's information
            char[] delimiters = { '}', '{', ':', '[', ']', ',', '\"', ';', '(', ')', '\n' };
            //storage for the words that are "read" in the item's information
            string[] word_array;
            //"cleaner" version of word_array
            List<string> words = new List<string>();


            //infinitely loop to allow for testing on any number of items
            while (true)
            {
                Console.WriteLine("Please give a server or type 'exit' to exit.");
                server = Console.ReadLine();
                if (server == "exit")
                    break;

                Console.WriteLine("Please give a character name.");
                char_name = Console.ReadLine();
                Console.WriteLine("\nNAME: {0}\nSERVER:{1}", char_name, server);

                //**********VULNERABILITY: COULD RUN INTO ISSUES WITH SERVERS WITH MORE THAN ONE SPACE IN THE NAME**********
                for (int i = 0; i < server.Length; i++)
                {
                    //if there's a space in the server name (i.e. "Area 52")
                    if (server[i].ToString() == " ")
                    {
                        for (int j = 0; j < server.Length; j++)
                        {
                            //need to turn the space into "%20"
                            if (server[j].ToString() == " ")
                                temp_server = temp_server + "%20";
                            else
                                temp_server = temp_server + server[j];
                        }
                        server = temp_server;
                    }
                }

                ////FORM URL 
                full_url = base_item_url + server + "/" + char_name + talent_url + api_key;
                //Console.WriteLine("URL: {0}\n", full_url);

                ////CALL API WITH FORMED URL
                WebRequest request = WebRequest.Create(full_url);
                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }
                //Console.WriteLine("RESPONSE: {0}\n", response);
                ////check that the id provided is valid
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine("That character/server combination does not exist");
                }
                else
                {
                    //COLLECT RELEVANT DATA FROM THE API
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    full_information = reader.ReadToEnd();

                    word_array = full_information.Split(delimiters);

                    //cleaning up the blank spots that tokenizing the original string created
                    for (int i = 0; i < word_array.Length; i++)
                    {
                        if (word_array[i] != "")
                            words.Add(word_array[i]);
                    }

                    //Console.WriteLine("EXTRACTED INFORMATION:");
                    //foreach (string w in words)
                    //{
                    //    Console.WriteLine(w);
                    //}

                    //TOKENIZE THE FULL_INFORMATION STRING TO FIND THE RELEVANT "KEYWORDS" FOR TESTING
                    for (int i = 0; i < words.Count(); i++)
                    {
                        //finding out the character's class
                        if(words[i] == "class")
                        {
                            if (words[i + 1] == "12")           //Demon Hunters have 2 specs and the class id "12"
                                num_specs = 2;
                            else if (words[i + 1] == "11")      //Druids have 4 specs and the class id "11"
                                num_specs = 4;
                            else                                //Every other class has 3 specs
                                num_specs = 3;
                        }

                        //PRIMARY TALENT INFORMATION EXTRACTION
                        //each talent's information starts with the word "tier"
                        if (words[i] == "tier" && primary_talents_done == false)
                        {

                            temp_tier = Convert.ToInt32(words[i + 1]);
                            int k = i + 1;


                            //
                            while (words[k] != "tier" && words[k] != "spec")
                            {
                                if (words[k] == "name")
                                    temp_name = words[k + 1];
                                if (words[k] == "icon")
                                    temp_icon = words[k + 1];

                                if (words[k] == "description")
                                {
                                    int j = k + 1;
                                    while (words[j] != "range" && words[j] != "castTime")
                                    {
                                        temp_desc = temp_desc + words[j] + " ";
                                        j++;
                                    }
                                    //temp_desc = words[i + 12];
                                }

                                k++;
                            }

                            Talent temp = new Talent(temp_tier, temp_name, temp_desc, temp_icon);
                            talents_first.Add(temp);
                            //need to "empty out" temp_desc between talents since it collects data through concatenation
                            temp_desc = "";
                        }
                        //***EXTRACTION OF SECONDARY TALENTS*******************************************************************************
                        else if (words[i] == "tier" && primary_talents_done == true && second_talents_done == false)
                        {

                            temp_tier = Convert.ToInt32(words[i + 1]);
                            int k = i + 1;


                            //
                            while (words[k] != "tier" && words[k] != "glyphs" && words[k] != "spec")
                            {
                                if (words[k] == "name")
                                    temp_name = words[k + 1];
                                if (words[k] == "icon")
                                    temp_icon = words[k + 1];

                                if (words[k] == "description")
                                {
                                    int j = k + 1;
                                    while (words[j] != "range" && words[j] != "castTime" && words[j] != "order")
                                    {
                                        temp_desc = temp_desc + words[j] + " ";
                                        j++;
                                    }
                                    //temp_desc = words[i + 12];
                                }

                                k++;
                            }

                            Talent temp = new Talent(temp_tier, temp_name, temp_desc, temp_icon);
                            talents_second.Add(temp);
                            temp_desc = "";
                        }
                        //************EXTRACTION OF THIRD SPEC TALENTS****************************************************************************
                        else if (words[i] == "tier" && primary_talents_done == true && second_talents_done == true && third_talents_done == false && num_specs > 2)
                        {
                            temp_tier = Convert.ToInt32(words[i + 1]);
                            int k = i + 1;

                            //
                            while (words[k] != "tier" && words[k] != "glyphs" && words[k] != "spec")
                            {
                                if (words[k] == "name")
                                    temp_name = words[k + 1];
                                if (words[k] == "icon")
                                    temp_icon = words[k + 1];

                                if (words[k] == "description")
                                {
                                    int j = k + 1;
                                    while (words[j] != "range" && words[j] != "castTime" && words[j] != "order")
                                    {
                                          temp_desc = temp_desc + words[j] + " ";
                                          j++;
                                    }
                                    //temp_desc = words[i + 12];
                                }

                                k++;
                            }

                            Talent temp = new Talent(temp_tier, temp_name, temp_desc, temp_icon);
                            talents_third.Add(temp);
                            temp_desc = "";
                        }
                        //************EXTRACTION OF FOURTH SPEC TALENTS***************************************************************************
                        else if (words[i] == "tier" && primary_talents_done == true && third_talents_done == true && num_specs == 4)
                        {
                            temp_tier = Convert.ToInt32(words[i + 1]);
                            int k = i + 1;


                            //
                            while (words[k] != "tier" && words[k] != "glyphs" && words[k] != "spec")
                            {
                                if (words[k] == "name")
                                    temp_name = words[k + 1];
                                if (words[k] == "icon")
                                    temp_icon = words[k + 1];

                                if (words[k] == "description")
                                {
                                    int j = k + 1;
                                    while (words[j] != "range" && words[j] != "castTime" && words[j] != "order")
                                    {
                                        temp_desc = temp_desc + words[j] + " ";
                                        j++;
                                    }
                                    //temp_desc = words[i + 12];
                                }

                                k++;
                            }

                            Talent temp = new Talent(temp_tier, temp_name, temp_desc, temp_icon);
                            talents_fourth.Add(temp);
                            temp_desc = "";
                        }

                        else if (words[i] == "spec")
                        {
                            if (primary_talents_done == false)
                            {
                                primary_spec = words[i + 2];
                            }
                            if (primary_talents_done == true && second_talents_done == false)
                            {
                                secondary_spec = words[i + 2];
                            }
                            if (second_talents_done == true && num_specs > 2 && third_talents_done == false)
                            {
                                third_spec = words[i + 2];
                            }
                            if (num_specs == 4 && third_talents_done == true)
                            {
                                fourth_spec = words[i + 2];
                            }
                        }
                        else if (words[i] == "calcSpec")
                        {
                            if (primary_talents_done == false)
                            {
                                primary_talents_done = true;
                            }
                            else if (primary_talents_done == true && second_talents_done == false)
                            {
                                second_talents_done = true;
                            }
                            else if (second_talents_done == true && num_specs > 2 && third_talents_done == false)
                            {
                                third_talents_done = true;
                            }
                        }

                        //end of extraction
                    }

                    //sort talent lists based on tier
                    talents_first.Sort(delegate(Talent x, Talent y)
                    {
                        return x.tier.CompareTo(y.tier);
                    });

                    talents_second.Sort(delegate(Talent x, Talent y)
                    {
                        return x.tier.CompareTo(y.tier);
                    });
                    talents_third.Sort(delegate (Talent x, Talent y)
                    {
                        return x.tier.CompareTo(y.tier);
                    });
                    talents_fourth.Sort(delegate (Talent x, Talent y)
                    {
                        return x.tier.CompareTo(y.tier);
                    });

                    //output talent lists
                    Console.WriteLine("\nPRIMARY SPEC: {0}\nTALENTS:\n", primary_spec);
                    for (int i = 0; i < talents_first.Count; i++)
                    {
                        talents_first[i].printInfo();
                    }
                    Console.WriteLine("\nSECONDARY SPEC: {0}\nTALENTS:\n", secondary_spec);
                    for (int i = 0; i < talents_second.Count; i++)
                    {
                        talents_second[i].printInfo();
                    }
                    if(num_specs > 2)
                    {
                        Console.WriteLine("\nTHIRD SPEC: {0}\nTALENTS:\n", third_spec);
                        for (int i = 0; i < talents_second.Count; i++)
                        {
                            talents_third[i].printInfo();
                        }
                    }
                    if (num_specs == 4)
                    {
                        Console.WriteLine("\nFOURTH SPEC: {0}\nTALENTS:\n", fourth_spec);
                        for (int i = 0; i < talents_second.Count; i++)
                        {
                            talents_fourth[i].printInfo();
                        }
                    }

                    words.Clear();

                }
                //reset information
                char_name = "";
                server = "";
                temp_server = "";
                full_url = "";

                talents_first.Clear();
                talents_second.Clear();
                talents_third.Clear();
                talents_fourth.Clear();
                primary_talents_done = false;
                second_talents_done = false;
                third_talents_done = false;

                Console.WriteLine("\n");
                //end of infinite loop
            }

            Console.WriteLine("Goodbye.");

        }
    }
}
