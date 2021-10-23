using System;
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
        /*A look up table to keep track of all teller total work time 
        To access teller id "9001238599" work time, use _tellerWorkTime["9001238599"] */
        private static Dictionary<string, double> _tellerWorkTime = new Dictionary<string, double>();
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
        //Intialize the teller work time table
        private static void initTellerWorkTime(TellerList tellers)
        {
            foreach (Teller teller in tellers.Teller)
            {
                _tellerWorkTime.Add(teller.id, 0);
            }
        }
        /* Since the goal is to minimize the work time of the bottleneck teller, this function will
        output the teller with minimum total work time given that the teller is assigned the input customer */
        private static Teller chooseBestTeller(Customer customer, TellerList tellers)
        {
            var minDuration = Double.MaxValue;
            var bestTeller = new Teller();
            var tieTeller = new List<Teller>();
            var appointmenDuration = 0.0;
            /*Go through each teller, calculate appointment duration assuming that the teller is assigned to the customer,
            and try to get the teller with the minimum total work time*/
            foreach (Teller teller in tellers.Teller)
            {
                if (customer.type == teller.specialtyType)
                {
                    appointmenDuration = Math.Ceiling(Convert.ToDouble(customer.duration) * Convert.ToDouble(teller.multiplier));
                }
                else
                {
                    appointmenDuration = Convert.ToDouble(customer.duration);
                }
                //Calculate this teller total work time up to this point, assume the teller is assigned to the customer
                var thisTellerTotalWorkTime = appointmenDuration + _tellerWorkTime[teller.id];
                //The best teller is the one with minimum total work time.
                if (minDuration > thisTellerTotalWorkTime)
                {
                    minDuration = thisTellerTotalWorkTime;
                    bestTeller = teller;
                }
            }
            //Update total work time if the teller is chosen. 
            _tellerWorkTime[bestTeller.id] += appointmenDuration;
            return bestTeller;
        }
        static void Calculation(CustomerList customers, TellerList tellers)
        {
            initTellerWorkTime(tellers);
            foreach (Customer customer in customers.Customer)
            {
                /*
                var appointment = new Appointment(customer, tellers.Teller[0]);
                appointmentList.Add(appointment);*/
                var bestTeller = chooseBestTeller(customer, tellers);
                var appointment = new Appointment(customer, bestTeller);
                appointmentList.Add(appointment);

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
