﻿using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DmvAppointmentScheduler
{
    class Program
    {
        public static Random random = new Random();
        public static List<Appointment> appointmentList = new List<Appointment>();

        //Keep track of the next available teller. The next available teller can be of any speciality type from (1,2,3,0)
        private static Dictionary<string, int> _nextAvailableTeller = new Dictionary<string, int>
        {
            { "tellerOneIndex", 0 },
            { "tellerTwoIndex", 0 },
            { "tellerThreeIndex", 0 },
            { "tellerZeroIndex", 0 },
        };
        static void Main(string[] args)
        {
            CustomerList customers = ReadCustomerData();
            TellerList tellers = ReadTellerData();
            Calculation(customers, tellers);
            OutputTotalLengthToConsole();

        }
        private static CustomerList ReadCustomerData()
        {
            string fileName = "CustomerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData", fileName);
            string jsonString = File.ReadAllText(path);
            CustomerList customerData = JsonConvert.DeserializeObject<CustomerList>(jsonString);
            return customerData;

        }
        private static TellerList ReadTellerData()
        {
            string fileName = "TellerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData", fileName);
            string jsonString = File.ReadAllText(path);
            TellerList tellerData = JsonConvert.DeserializeObject<TellerList>(jsonString);
            return tellerData;

        }
        /*This function classify teller based on speciality type
        and return a look up table with the key is teller type, 
        the value is the list of teller belong to that type
        For example, to access a list of teller type 1: 
        tellerClassifer["specialtyType_1"]*/
        private static Dictionary<string, List<Teller>> classifyTeller(TellerList tellers)
        {
            List<Teller> tellerOneList = new List<Teller>();
            List<Teller> tellerTwoList = new List<Teller>();
            List<Teller> tellerThreeList = new List<Teller>();
            List<Teller> tellerZeroList = new List<Teller>();
            foreach (Teller teller in tellers.Teller)
            {
                if (teller.specialtyType == "1")
                {
                    tellerOneList.Add(teller);
                }
                else if (teller.specialtyType == "2")
                {
                    tellerTwoList.Add(teller);
                }
                else if (teller.specialtyType == "3")
                {
                    tellerThreeList.Add(teller);
                }
                else if (teller.specialtyType == "0")
                {
                    tellerZeroList.Add(teller);
                }
            }
            Dictionary<string, List<Teller>> tellerClassifer = new Dictionary<string, List<Teller>>
            {   { "specialtyType_1", tellerOneList },
                { "specialtyType_2", tellerTwoList },
                { "specialtyType_3", tellerThreeList },
                { "specialtyType_0", tellerZeroList },
            };
            return tellerClassifer;

        }
        //Check the customer type and assign to the correct type of teller speciality
        private static void assignCustomerToTeller(Customer customer, Dictionary<string, List<Teller>> tellerClassifer)
        {
            if (customer.type == "1")
            {
                //Get the next available teller with speciality one
                var nextTellerOneIndex = _nextAvailableTeller["tellerOneIndex"];
                var nextAvailableTellerOne = tellerClassifer["specialtyType_1"][nextTellerOneIndex];
                var appointment = new Appointment(customer, nextAvailableTellerOne);
                appointmentList.Add(appointment);
                _nextAvailableTeller["tellerOneIndex"]++;
                var totalNumberOfTellerOne = tellerClassifer["specialtyType_1"].Count();
                if (_nextAvailableTeller["tellerOneIndex"] >= totalNumberOfTellerOne)
                {
                    _nextAvailableTeller["tellerOneIndex"] = 0;
                }
            }
            else if (customer.type == "2")
            {
                //Get the next available teller with speciality two
                var nextTellerTwoIndex = _nextAvailableTeller["tellerTwoIndex"];
                var nextAvailableTellerTwo = tellerClassifer["specialtyType_2"][nextTellerTwoIndex];
                var appointment = new Appointment(customer, nextAvailableTellerTwo);
                appointmentList.Add(appointment);
                _nextAvailableTeller["tellerTwoIndex"]++;
                var totalNumberOfTellerTwo = tellerClassifer["specialtyType_2"].Count();
                if (_nextAvailableTeller["tellerTwoIndex"] >= totalNumberOfTellerTwo)
                {
                    _nextAvailableTeller["tellerTwoIndex"] = 0;
                }
            }
            else if (customer.type == "3")
            {
                //Get the next available teller with speciality three
                var nextTellerThreeIndex = _nextAvailableTeller["tellerThreeIndex"];
                var nextAvailableTellerThree = tellerClassifer["specialtyType_3"][nextTellerThreeIndex];
                var appointment = new Appointment(customer, nextAvailableTellerThree);
                appointmentList.Add(appointment);
                _nextAvailableTeller["tellerThreeIndex"]++;
                var totalNumberOfTellerThree = tellerClassifer["specialtyType_3"].Count();
                if (_nextAvailableTeller["tellerThreeIndex"] >= totalNumberOfTellerThree)
                {
                    _nextAvailableTeller["tellerThreeIndex"] = 0;
                }
            }
            else if (customer.type == "4")
            {
                //Get the next available teller with speciality zero
                var nextTellerZeroIndex = _nextAvailableTeller["tellerZeroIndex"];
                var nextAvailableTellerZero = tellerClassifer["specialtyType_0"][nextTellerZeroIndex];
                var appointment = new Appointment(customer, nextAvailableTellerZero);
                appointmentList.Add(appointment);
                _nextAvailableTeller["tellerZeroIndex"]++;
                var totalNumberOfTellerZero = tellerClassifer["specialtyType_0"].Count();
                if (_nextAvailableTeller["tellerZeroIndex"] >= totalNumberOfTellerZero)
                {
                    _nextAvailableTeller["tellerZeroIndex"] = 0;
                }
            }
            else
            {
                Console.WriteLine("Error! Customer type does not recognized ");
            }
        }
        static void Calculation(CustomerList customers, TellerList tellers)
        {
            /*It's important to match customer with the correct teller speciality type to reduce the waiting time
              Notice that teller has type (1,2,3,0)
              and Customer has type (1,2,3,4)
              So we can have the following mapping
              Customer    Teller
                 1       ->   1
                 2       ->   2
                 3       ->   3
                 4       ->   0*/
            var tellerClassifer = classifyTeller(tellers);
            foreach (Customer customer in customers.Customer)
            {
                /*
                var appointment = new Appointment(customer, tellers.Teller[0]);
                appointmentList.Add(appointment);*/
                assignCustomerToTeller(customer, tellerClassifer);
            }
        }
        static void OutputTotalLengthToConsole()
        {
            var tellerAppointments =
                from appointment in appointmentList
                group appointment by appointment.teller into tellerGroup
                select new
                {
                    teller = tellerGroup.Key,
                    totalDuration = tellerGroup.Sum(x => x.duration),
                };
            var max = tellerAppointments.OrderBy(i => i.totalDuration).LastOrDefault();
            Console.WriteLine("Teller " + max.teller.id + " will work for " + max.totalDuration + " minutes!");
        }

    }
}
