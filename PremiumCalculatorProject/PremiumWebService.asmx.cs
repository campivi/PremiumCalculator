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
    /// Also Validates data and can retrieve it for different purposes
    /// </summary>

    [WebService(Namespace = "http://www.premiumwebservice.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class PremiumWebService : System.Web.Services.WebService
    {
        public static List<premiumData> premiumList = new List<premiumData>();

        /* THIS METHOD SHOWS THE MAIN CORE CALCULATION FOR THE PROJECT */
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

        /* THIS METHOD POPULATES THE STATES COMBO BOX */
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

        /* THIS METHOD POPULATES THE PLANS COMBO BOX */
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetPlans()
        {
            var states = premiumList.GroupBy(it => new { it.plan })
                                    .Select(it => it.FirstOrDefault())
                                    .ToList();

            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(states.OrderBy(it => it.plan));
        }

        /* THIS METHOD RETRIEVES THE SET OF RULES USED */
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetPremiumRules()
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(premiumList);
        }

        /* THIS METHOD ADDS MORE RULES */
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AddPremiumList(string carrier, string plan, string state, string stateName, string month, string minAge, string maxAge, string premium)
        {
            try
            {
                premiumData newData = new premiumData();

                newData.carrier = ValidateCarrier(carrier);
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
                    throw new Exception("Duplicated entry");
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

        /* THIS METHOD EDITS A RULE */
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string EditPremiumList(string id, string carrier, string plan, string state, string stateName, string month, string minAge, string maxAge, string premium)
        {
            try
            {
                int idWS = ValidateAge(id);
                var oldData = premiumList.FirstOrDefault(it => it.id == idWS);

                if (!hasExactMatchFull(carrier, ValidatePlan(plan), ValidateState(state), 
                                       ValidateStateName(state, stateName), ValidateMonth(month), 
                                       ValidateAge(minAge), ValidateAge(maxAge), ValidatePremium(premium)))
                {

                    oldData.carrier = ValidateCarrier(carrier);
                    oldData.plan = ValidatePlan(plan);
                    oldData.state = ValidateState(state);
                    oldData.stateName = ValidateStateName(state, stateName);
                    oldData.month = ValidateMonth(month);
                    oldData.minAge = ValidateAge(minAge);
                    oldData.maxAge = ValidateAge(maxAge);
                    oldData.premium = ValidatePremium(premium);

                    var errorMsg = new errorResponse();
                    errorMsg.status = "success";
                    errorMsg.message = "Edited Succesfully";
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    return js.Serialize(errorMsg);
                }
                else
                {
                    throw new Exception("Duplicated entry");
                }
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

        /* THIS METHOD DELETES A RULE */
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

        /* THIS METHOD PROVIDES THE RULE INFORMATION TO EDIT */
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string EditPremiumRule(string id)
        {
            int idWS = ValidateAge(id);
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(premiumList.FirstOrDefault(it => it.id == idWS));
        }

        /* THIS METHOD LOADS THE SAME TEST DATA AS THE PDF SHOWED BY EMAIL
         * ONLY USED THIS TO TEST FASTER THE FUNCTIONALITIES
         * SINCE THE SET OF RULES SHOULD START EMPTY
         */
        [WebMethod]
        [ScriptMethod(ResponseFormat =ResponseFormat.Json)]
        public string LoadTestData()
        {
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

            var errorMsg = new errorResponse();
            errorMsg.status = "success";
            errorMsg.message = "Sample Data Loaded";
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(errorMsg);
        }


        /* HERE ARE ALL THE VALIDATIONS USED */
        private string ValidateCarrier(string carrier)
        {
            if (string.IsNullOrEmpty(carrier))
                throw new Exception("Please fill empty fields");
            if (!Regex.IsMatch(carrier, "^[a-zA-Z ]+$"))
                throw new Exception("State Name does not match only letters format");

            return carrier;

        }
        private DateTime ValidateDateOfBirth(string dateofbirth)
        {
            DateTime dob;
            if (string.IsNullOrEmpty(dateofbirth))
                throw new Exception("Please fill empty fields");
            if (!DateTime.TryParse(dateofbirth, out dob))
                throw new Exception("Date of Birth does not match DateTime format");

            return dob;
        }
        private string ValidateState(string state)
        {
            if (string.IsNullOrEmpty(state))
                throw new Exception("Please fill empty fields");
            if (state.Length > 2)
                throw new Exception("State length exceeds the two characters limit");
            if (state != "*" && !Regex.IsMatch(state, "^[a-zA-Z]+$"))
                throw new Exception("State does not match only letters format");

            return state;
        }
        private string ValidateStateName(string state, string stateName)
        {
            if (state == "*")
                return "";
            else
            {
                if (string.IsNullOrEmpty(stateName))
                    throw new Exception("Please fill empty fields");
                if (!Regex.IsMatch(stateName, "^[a-zA-Z ]+$"))
                throw new Exception("State Name does not match only letters format");

                return stateName;
            }
        }
        private int ValidateAge(string age)
        {
            if (string.IsNullOrEmpty(age))
                throw new Exception("Please fill empty fields");
            int ageWS;
            if (!int.TryParse(age, out ageWS))
                throw new Exception("Age does not match Integer format");

            return ageWS;
        }
        private string ValidateMonth(string month)
        {
            if (string.IsNullOrEmpty(month))
                throw new Exception("Please fill empty fields");
            if (month == "*")
                return month;
            else
            {
                int monthWS;
                if (!int.TryParse(month, out monthWS))
                    throw new Exception("Month does not match Integer format");
                if(monthWS > 12 || monthWS < 0)
                    throw new Exception("Month " + monthWS.ToString() + " doesn't exist");

                return monthWS.ToString();
            }
        }
        private char ValidatePlan(string plan)
        {
            char planWS;
            if(string.IsNullOrEmpty(plan))
                throw new Exception("Please fill empty fields");
            if (!char.TryParse(plan, out planWS))
                throw new Exception("Plan length exceeds the one character limit");
            if (!Regex.IsMatch(plan, "^[a-zA-Z]+$"))
                throw new Exception("Plan does not match only letters format");

            return planWS;
        }
        private string ValidatePremium(string premium)
        {
            if (string.IsNullOrEmpty(premium))
                throw new Exception("Please fill empty fields");
            decimal premiumWS;
            if (!decimal.TryParse(premium, out premiumWS))
                throw new Exception("Premium does not match Decimal format");

            return string.Format("{0:#.00}", premiumWS);
        }
        private bool hasExactMatch(premiumData data)
        {
            return premiumList.Any(it => it.carrier == data.carrier && it.maxAge == data.maxAge
                                      && it.minAge == data.minAge && it.month == data.month
                                      && it.plan == data.plan && it.premium == data.premium
                                      && it.state == data.state && it.stateName == data.stateName);
        }
        private bool hasExactMatchFull(string carrier, char plan, string state, string stateName, string month, int minAge, int maxAge, string premium)
        {
            return premiumList.Any(it => it.carrier == carrier && it.maxAge == maxAge
                                      && it.minAge == minAge && it.month == month
                                      && it.plan == plan && it.premium == premium
                                      && it.state == state && it.stateName == stateName);
        }


        /* MORE FUNCTIONS USED IN THE PROJECT */
        private int GetAge(DateTime dateofbirth)
        {
            DateTime today = DateTime.Now;

            var age = today.Year - dateofbirth.Year;
            var monthDiff = today.Month - dateofbirth.Month;
            if (monthDiff < 0 || (monthDiff == 0 && today < dateofbirth))
                age--;

            return age;
        }
        private List<response> GetPremiumPlan(int month, string state, int age, char plan)
        {
            List<response> premiumPlans = new List<response>();

            var searchedPlan = premiumList.Where(it => it.plan == plan).ToList();
            var searchedAge = searchedPlan.Where(it => it.minAge <= age && it.maxAge >= age).ToList();
            var searchState = searchedAge.Where(it => it.state == state || it.state == "*").ToList();
            var resultList = searchState.Where(it => it.month == month.ToString() || it.month == "*").ToList();

            foreach(var item in resultList)
            {
                var premPlan = new response();
                premPlan.carrier = item.carrier;
                premPlan.premium = item.premium;
                premiumPlans.Add(premPlan);
            }

            return premiumPlans;
        }
    }
}
