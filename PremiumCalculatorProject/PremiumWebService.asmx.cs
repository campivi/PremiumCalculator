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
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string PremiumCalculator(string dateofbirth, string state, string age, string plan)
        {
            try
            {
                /* VALIDATE ALL INPUT DATA*/
                // DATE OF BIRTH
                DateTime dob;
                if (!DateTime.TryParse(dateofbirth, out dob))
                    throw new Exception("Date of Birth does not match DateTime format");

                // STATE
                if (state.Length > 2)
                    throw new Exception("State length exceeds the two characters limit");
                if (!Regex.IsMatch(state, "^[a-zA-Z]+$"))
                    throw new Exception("State does not match only letters format");

                // AGE
                int ageWS;
                if (!int.TryParse(age, out ageWS))
                    throw new Exception("Age does not match Integer format");

                // PLAN
                char planWS;
                if (!char.TryParse(plan, out planWS))
                    throw new Exception("Plan length exceeds the one character limit");
                if (!Regex.IsMatch(plan, "^[a-zA-Z]+$"))
                    throw new Exception("Plan does not match only letters format");


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
                    //Context.Response.Write(js.Serialize(premiumPlans));
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
                //Context.Response.Write(js.Serialize(errorMsg));
                return js.Serialize(errorMsg);

            }
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

        public List<response> GetPremiumPlan(int month, string state, int age, char plan)
        {
            List<response> premiumPlans = new List<response>();
            var premPlan = new response();

            //FIRST STEP IS TO LOCATE THE PLAN...
            //    reason: the plan must be found logically following a decision tree, the only parameter we got that
            //            do not change are the PLANs, we have only tree option. Switch can only be use for the plan
            //            because the response can contain more than one answers, IFs are more appropiate to not skip 
            //            possible other results.
            switch (plan)
            {
                case 'A':
                    #region PLAN A

                    #region NEW YORK
                    if (state == "NY")
                    {
                        if (age >= 21)
                        {
                            if (month == 9)
                            {
                                if (age <= 45)
                                {
                                    premPlan = new response();
                                    premPlan.carrier = "Qwerty";
                                    premPlan.premium = "150.00";
                                    premiumPlans.Add(premPlan);
                                }
                            }

                            if (month == 10)
                            {
                                if (age <= 65)
                                {
                                    premPlan = new response();
                                    premPlan.carrier = "Asdf";
                                    premPlan.premium = "150.00";
                                    premiumPlans.Add(premPlan);
                                }
                            }
                        }

                        if (age >= 18 || age <= 65)
                        {
                            premPlan = new response();
                            premPlan.carrier = "Qwerty";
                            premPlan.premium = "120.99";
                            premiumPlans.Add(premPlan);

                            premPlan = new response();
                            premPlan.carrier = "Asdf";
                            premPlan.premium = "129.95";
                            premiumPlans.Add(premPlan);
                        }
                    }
                    #endregion

                    #region ALASKA
                    if (state == "AK")
                    {
                        if (month == 11)
                        {
                            if (age >= 18 || age <= 64)
                            {
                                premPlan = new response();
                                premPlan.carrier = "Qwerty";
                                premPlan.premium = "125.16";
                                premiumPlans.Add(premPlan);
                            }
                            if (age >= 65 || age <= 120)
                            {
                                premPlan = new response();
                                premPlan.carrier = "Qwerty";
                                premPlan.premium = "175.20";
                                premiumPlans.Add(premPlan);
                            }
                        }
                    }
                    #endregion

                    if (age >= 18 || age <= 65)
                    {
                        #region ALABAMA
                        if (state == "AL")
                        {
                            if (month == 11)
                            {
                                premPlan = new response();
                                premPlan.carrier = "Qwerty";
                                premPlan.premium = "85.50";
                                premiumPlans.Add(premPlan);

                                premPlan = new response();
                                premPlan.carrier = "Qwerty";
                                premPlan.premium = "84.50";
                                premiumPlans.Add(premPlan);
                            }
                        }
                        #endregion

                        #region ANYWHERE
                        premPlan = new response();
                        premPlan.carrier = "Qwerty";
                        premPlan.premium = "90";
                        premiumPlans.Add(premPlan);

                        premPlan = new response();
                        premPlan.carrier = "Asdf";
                        premPlan.premium = "89.99";
                        premiumPlans.Add(premPlan);

                        #endregion
                    }
                    break;

                #endregion
                case 'B':
                    #region PLAN B

                    if (age <= 65)
                    {
                        if (age >= 46)
                        {
                            #region NEW YORK
                            if (state == "NY")
                            {
                                if (month == 1)
                                {
                                    premPlan = new response();
                                    premPlan.carrier = "Qwerty";
                                    premPlan.premium = "200.50";
                                    premiumPlans.Add(premPlan);

                                    premPlan = new response();
                                    premPlan.carrier = "Asdf";
                                    premPlan.premium = "184.50";
                                    premiumPlans.Add(premPlan);

                                }
                            }
                        }
                        #endregion

                        if (age >= 18)
                        {
                            #region ALASKA
                            if (state == "AK")
                            {
                                premPlan = new response();
                                premPlan.carrier = "Qwerty";
                                premPlan.premium = "100.80";
                                premiumPlans.Add(premPlan);

                                premPlan = new response();
                                premPlan.carrier = "Asdf";
                                premPlan.premium = "100.80";
                                premiumPlans.Add(premPlan);

                            }
                            #endregion

                            #region WYOMING
                            if (state == "WY")
                            {
                                premPlan = new response();
                                premPlan.carrier = "Qwerty";
                                premPlan.premium = "100.00";
                                premiumPlans.Add(premPlan);
                            }
                            #endregion
                        }
                    }

                    #endregion
                    break;
                case 'C':
                    #region PLAN C

                    if (age >= 18 || age <= 65)
                    {
                        #region NEW YORK
                        if (state == "NY")
                        {
                            premPlan = new response();
                            premPlan.carrier = "Qwerty";
                            premPlan.premium = "120.99";
                            premiumPlans.Add(premPlan);
                        }
                        #endregion

                        #region ALABAMA
                        if (state == "AL")
                        {
                            premPlan = new response();
                            premPlan.carrier = "Qwerty";
                            premPlan.premium = "100.00";
                            premiumPlans.Add(premPlan);

                        }
                        #endregion

                        #region ALASKA
                        if (state == "AK")
                        {
                            premPlan = new response();
                            premPlan.carrier = "Asdf";
                            premPlan.premium = "100.80";
                            premiumPlans.Add(premPlan);
                        }
                        #endregion

                        #region ANYWHERE

                        premPlan = new response();
                        premPlan.carrier = "Qwerty";
                        premPlan.premium = "90.00";
                        premiumPlans.Add(premPlan);

                        premPlan = new response();
                        premPlan.carrier = "Asdf";
                        premPlan.premium = "89.99";
                        premiumPlans.Add(premPlan);

                        #endregion
                    }
                    break;

                    #endregion
            }

            return premiumPlans;
        }
    }
}
