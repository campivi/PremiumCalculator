using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Linq;

namespace PremiumProject
{
    public class premiumData
    {
        public int id { get; set; }
        public string carrier { get; set; }
        public char plan { get; set; }
        public string state { get; set; }
        public string stateName { get; set; }
        public string month { get; set; }
        public int minAge { get; set; }
        public int maxAge { get; set; }
        public string premium { get; set; }
    }
    public class response
    {
        public string carrier { get; set; }
        public string premium { get; set; }
    }
    public class errorResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }
    /// <summary>
    /// Provides a calculation of the Premium Plan that better fits the clients data
    /// </summary>

    [WebService(Namespace = "http://www.premiumwebservice.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class PremiumWebService : System.Web.Services.WebService
    {
        public static List<premiumData> premiumList = new List<premiumData>();
        public PremiumWebService()
        {
            /*
            AddPremiumList("Querty", "A", "NY", "New York", "9", "21", "45", "150");
            AddPremiumList("Querty", "B", "NY", "New York", "1", "46", "65", "200.4");
            AddPremiumList("Querty", "A", "NY", "New York", "*", "18", "65", "120.99");
            AddPremiumList("Querty", "C", "NY", "New York", "*", "18", "65", "120.99");
            AddPremiumList("Querty", "A", "AL", "Alabama", "11", "18", "65", "85.5");
            AddPremiumList("Querty", "C", "AL", "Alabama", "*", "18", "65", "100");
            AddPremiumList("Querty", "A", "AK", "Alaska", "12", "65", "120", "175.2");
            AddPremiumList("Querty", "A", "AK", "Alaska", "12", "18", "64", "125.16");
            AddPremiumList("Querty", "B", "AK", "Alaska", "*", "18", "65", "100.8");
            AddPremiumList("Querty", "A", "AK", "Alaska", "*", "18", "65", "90");
            AddPremiumList("Querty", "C", "*", "", "*", "18", "65", "90");
            AddPremiumList("Asdf", "A", "NY", "New York", "10", "21", "65", "150");
            AddPremiumList("Asdf", "B", "NY", "New York", "1", "46", "65", "184.5");
            AddPremiumList("Asdf", "A", "NY", "New York", "*", "18", "65", "129.95");
            AddPremiumList("Asdf", "A", "AL", "Alabama", "11", "18", "65", "84.5");
            AddPremiumList("Asdf", "B", "WY", "Wyoming", "*", "18", "65", "100");
            AddPremiumList("Asdf", "B", "AK", "Alaska", "*", "18", "65", "100.8");
            AddPremiumList("Asdf", "C", "AK", "Alaska", "*", "18", "65", "100.8");
            AddPremiumList("Asdf", "A", "*", "", "*", "18", "65", "89.99");
            AddPremiumList("Asdf", "C", "*", "", "*", "18", "65", "89.99");
            */
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string PremiumCalculator(string dateofbirth, string state, string age, string plan)
        {
            try
            {
                /* VALIDATE ALL INPUT DATA*/

                // DATE OF BIRTH
                DateTime dob = ValidateDateOfBirth(dateofbirth);
                // STATE
                state = ValidateState(state);
                // AGE
                int ageWS = ValidateAge(age);
                // PLAN
                char planWS = ValidatePlan(plan);

                /* DATA SHOULD BE CORRECT NOW, SO WE CAN START THE CALCULATIONS*/
                // AGE SHOULD MATCH THE INPUT DATA
                var calculatedAge = GetAge(dob);
                if (calculatedAge != ageWS)
                    throw new Exception("Calculated age does not match input data");

                List<response> premiumPlans = new List<response>();
                premiumPlans = GetPremiumPlan(dob.Month, state, ageWS, planWS);

                if (premiumPlans.Count > 0)
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    return js.Serialize(premiumPlans.OrderBy(it => it.carrier));
                }
                else
                    throw new Exception("Couldn't match the data to any premium plan, please try again");
            }
            catch (Exception ex)
            {
                var errorMsg = new errorResponse();
                errorMsg.status = "failed";
                errorMsg.message = ex.Message;
                JavaScriptSerializer js = new JavaScriptSerializer();
                return js.Serialize(errorMsg);
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetStates()
        {
            var restricted = premiumList.Where(it => it.state != "*").ToList();
            var states = restricted.GroupBy(it => new { it.state, it.stateName})
                                    .Select(it => it.FirstOrDefault())
                                    .ToList();

            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(states);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetPremiumRules()
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(premiumList);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AddPremiumList(string carrier, string plan, string state, string stateName, string month, string minAge, string maxAge, string premium)
        {
            try
            {
                premiumData newData = new premiumData();

                newData.carrier = carrier;
                newData.plan = ValidatePlan(plan);
                newData.state = ValidateState(state);
                newData.stateName = ValidateStateName(state, stateName);
                newData.month = ValidateMonth(month);
                newData.minAge = ValidateAge(minAge);
                newData.maxAge = ValidateAge(maxAge);
                newData.premium = ValidatePremium(premium);
                newData.id = premiumList.Count > 0 ? premiumList.Max(it => it.id) + 1 : 1;

                if (!hasExactMatch(newData))
                {
                    premiumList.Add(newData);

                    var errorMsg = new errorResponse();
                    errorMsg.status = "success";
                    errorMsg.message = "Added Succesfully";
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    return js.Serialize(errorMsg);
                }
                else
                {
                    var errorMsg = new errorResponse();
                    errorMsg.status = "success";
                    errorMsg.message = "Duplicated data";
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    return js.Serialize(errorMsg);
                }
            } 
            catch(Exception ex)
            {
                var errorMsg = new errorResponse();
                errorMsg.status = "failed";
                errorMsg.message = ex.Message;
                JavaScriptSerializer js = new JavaScriptSerializer();
                return js.Serialize(errorMsg);
            }
        }
        
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string DeletePremiumRule(string id)
        {
            int idWS = ValidateAge(id);
            var deleteItem = premiumList.FirstOrDefault(it => it.id == idWS);
            premiumList.Remove(deleteItem);

            var errorMsg = new errorResponse();
            errorMsg.status = "success";
            errorMsg.message = "Eliminated";
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(errorMsg);
        }

        public int GetAge(DateTime dateofbirth)
        {
            DateTime today = DateTime.Now;

            var age = today.Year - dateofbirth.Year;
            var monthDiff = today.Month - dateofbirth.Month;
            if (monthDiff < 0 || (monthDiff == 0 && today < dateofbirth))
                age--;

            return age;
        }
        private DateTime ValidateDateOfBirth(string dateofbirth)
        {
            DateTime dob;
            if (!DateTime.TryParse(dateofbirth, out dob))
                throw new Exception("Date of Birth does not match DateTime format");

            return dob;
        }
        private string ValidateState(string state)
        {
            if (state.Length > 2)
                throw new Exception("State length exceeds the two characters limit");
            if (state != "*" && !Regex.IsMatch(state, "^[a-zA-Z]+$"))
                throw new Exception("State does not match only letters format");

            return state;
        }
        private string ValidateStateName(string state, string stateName)
        {
            if (state != "*" && !Regex.IsMatch(stateName, "^[a-zA-Z]+$"))
                throw new Exception("State Name does not match only letters format");

            if (state == "*")
                return "";
            else
                return stateName;
        }
        private int ValidateAge(string age)
        {
            int ageWS;
            if (!int.TryParse(age, out ageWS))
                throw new Exception("Age does not match Integer format");

            return ageWS;
        }
        private string ValidateMonth(string month)
        {
            if (month == "*")
                return month;
            else
            {
                int monthWS;
                if (!int.TryParse(month, out monthWS))
                    throw new Exception("Month does not match Integer format");

                return monthWS.ToString();
            }
        }
        private char ValidatePlan(string plan)
        {
            char planWS;
            if (!char.TryParse(plan, out planWS))
                throw new Exception("Plan length exceeds the one character limit");
            if (!Regex.IsMatch(plan, "^[a-zA-Z]+$"))
                throw new Exception("Plan does not match only letters format");

            return planWS;
        }
        private string ValidatePremium(string premium)
        {
            decimal premiumWS;
            if (!decimal.TryParse(premium, out premiumWS))
                throw new Exception("Premium does not match Decimal format");

            return string.Format("{0:#.00}", premiumWS);
        }

        public bool hasExactMatch(premiumData data)
        {
            return premiumList.Any(it => it.carrier == data.carrier && it.maxAge == data.maxAge
                                      && it.minAge == data.minAge && it.month == data.month
                                      && it.plan == data.plan && it.premium == data.premium
                                      && it.state == data.state && it.stateName == data.stateName);
        }
        public List<response> GetPremiumPlan(int month, string state, int age, char plan)
        {
            List<response> premiumPlans = new List<response>();
            var premPlan = new response();

            var resultList = premiumList.Where(it => it.plan == plan 
                                           && (it.minAge >= age && it.maxAge <= age)
                                           && (it.state == state || it.state == "*") 
                                           && (it.month == month.ToString() || it.month == "*")).ToList();

            return premiumPlans;
        }
    }
}
