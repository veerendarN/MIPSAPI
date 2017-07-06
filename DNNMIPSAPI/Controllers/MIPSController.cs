using DNNAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Data;

namespace DNNMIPSAPI.Controllers
{
    public class MIPSController : ApiController
    {
        [HttpGet]
        public string hello()
        {
            return "success";
        }
        [HttpGet]
        public List<MeasureData> GetAllMeasures()
        {
            var MeasureDataList = new List<MeasureData>();
            using (var entity = new DNNMIPSEntities())
            {
                var result = entity.vw_QPPMeasures_With_Modality.GroupBy(m => m.Measure_Num);
                MeasureDataList = result.Select(mes => new MeasureData
                {
                    MeasureNumber = mes.FirstOrDefault().Measure_Num,
                    MeasureTitle = mes.FirstOrDefault().Measure_Title,
                    messageDesc = mes.FirstOrDefault().Measure_Description,
                    DisplayOrder = mes.FirstOrDefault().DisplayOrder,
                    MeasureTypeDesc = mes.FirstOrDefault().Message_Desc,
                    MopdalityDesc = mes.FirstOrDefault().Modality,
                    SpecialtyDesc = mes.FirstOrDefault().specialty_Desc,
                    measureTypeCode = mes.FirstOrDefault().Measure_Type_Code,
                    measure_priority = mes.FirstOrDefault().Measure_Priority,
                    Measure_URL = mes.FirstOrDefault().Measure_URL,
                    Message2 = mes.FirstOrDefault().Message2

                }).OrderBy(m => m.DisplayOrder).ToList();
            }
            return MeasureDataList;
        }

        [HttpGet]
        public List<List<string>> GetAllLookUpsData()
        {
            var globalList = new List<List<string>>();
            var listRegistgryStrings = new List<string>();

            using (var entity = new DNNMIPSEntities())
            {

                listRegistgryStrings = entity.tbl_Lookup_Specialty.Select(i => i.Description).ToList();
                globalList.Add(listRegistgryStrings);
                listRegistgryStrings = new List<string>();


                listRegistgryStrings = entity.tbl_Lookup_QPP_Modality.Select(i => i.Modality).ToList();
                globalList.Add(listRegistgryStrings);
                listRegistgryStrings = new List<string>();

                listRegistgryStrings = entity.tbl_Lookup_Registry.Select(i => i.Description).ToList();
                listRegistgryStrings.RemoveAt(5);
                listRegistgryStrings.Insert(0, "Merit-Based Incentive Payment System");
                globalList.Add(listRegistgryStrings);



                listRegistgryStrings = entity.tbl_Lookup_QPP_Measure_Type.Select(i => i.Description).ToList();
                globalList.Add(listRegistgryStrings);
                listRegistgryStrings = new List<string>();
                return globalList;

            }

        }
        [HttpGet]
        public List<MeasureData> GetAllSelectedMeasures(string arrayOfValues)
        {

            var MeasureDataList = new List<MeasureData>();

            var listarray = arrayOfValues.Split(',').Select(i => i).ToArray();

            try
            {
                using (var entity = new DNNMIPSEntities())
                {

                    var mtc = (from n in entity.tbl_Lookup_QPP_Measure_Type where listarray.Contains(n.Description) select n.QPP_Measure_Type_Code).ToList();

                    if (mtc.Count > 0)
                    {

                        MeasureDataList = (from m in entity.vw_QPPMeasures_With_Modality
                                           where mtc.Contains(m.Measure_Type_Code)
                                           select new MeasureData
                                           {
                                               MeasureNumber = m.Measure_Num,
                                               MeasureTitle = m.Measure_Title,
                                               DisplayOrder = m.DisplayOrder,

                                           }).ToList();

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return MeasureDataList;
        }


        [HttpGet]
        public List<MeasureData> GetSelectionMeasures(string strMeasureType, string strModality = "", string strRegistry = "", string strSpecialty = "")
        {
            var measuresList = new List<MeasureData>();
            var mtc = new List<string>();
            var md = new List<string>();
            var reg = new List<string>();
            var spec = new List<string>();
            var listarray = new List<string>();
            var listarraymd = new List<string>();
            var listarrayreg = new List<string>();
            var listarrayspec = new List<string>();
            var blnContainsHP = false;
            var mesNumList = new List<string>();
            var mesNumList2 = new List<string>();
            var strMessage = "";

            using (var entity = new DNNMIPSEntities())
            {
                mesNumList = (from m in entity.vw_QPPMeasures_With_Modality select m.Measure_Num).Distinct().ToList();
                if (mesNumList.Count > 0)
                {
                    mesNumList2 = mesNumList;

                    // measure code type
                    if (!string.IsNullOrEmpty(strMeasureType))
                    {
                        listarray = strMeasureType.Split(',').Select(i => i).ToList();
                        mtc = (from n in entity.tbl_Lookup_QPP_Measure_Type where listarray.Contains(n.Description) select n.QPP_Measure_Type_Code).ToList();
                        foreach (var mt in mtc)
                        {

                            if (mt.Equals("HP"))
                            {
                                blnContainsHP = true;

                            }
                        }


                        if (blnContainsHP)
                        {
                            var hpList = new List<string>();
                            hpList.Add("!");
                            hpList.Add("!!");
                            mesNumList = (from mtlist in entity.vw_QPPMeasures_With_Modality
                                          where (
                                            (mtc.Contains(mtlist.Measure_Type_Code)
                                            || (hpList.Contains(mtlist.Measure_Priority))
                                            ) && mesNumList2.Contains(mtlist.Measure_Num)
                                            )
                                          select mtlist.Measure_Num).Distinct().ToList();
                        }
                        else
                        {
                            mesNumList = (from mtlist in entity.vw_QPPMeasures_With_Modality
                                          where (
                                            mtc.Contains(mtlist.Measure_Type_Code)
                                             && mesNumList2.Contains(mtlist.Measure_Num)
                                            )
                                          select mtlist.Measure_Num).Distinct().ToList();
                            strMessage += "mtype: " + strModality + ":" + Convert.ToString(mesNumList.Count) + " Arraylist: " + Convert.ToString(listarraymd.Count);
                        }

                        mesNumList2 = mesNumList;

                    }


                    // modality
                    if (!string.IsNullOrEmpty(strModality))
                    {
                        listarraymd = strModality.Split(',').Select(i => i).ToList();
                        mesNumList = (from vmmodality in entity.vw_QPPMeasures_With_Modality
                                      where listarraymd.Contains(vmmodality.Modality)
                                      && mesNumList2.Contains(vmmodality.Measure_Num)
                                      select vmmodality.Measure_Num).Distinct().ToList();

                        strMessage += "Modality: " + strModality + ":" + Convert.ToString(mesNumList.Count) + " Arraylist: " + Convert.ToString(listarraymd.Count);
                        mesNumList2 = mesNumList;
                    }

                    //registry
                    if (!string.IsNullOrEmpty(strRegistry))
                    {
                        var Descriptonlist = new List<string>();
                        Descriptonlist = strRegistry.Split(',').Select(i => i).ToList(); //Description list.
                        Descriptonlist = Descriptonlist.Select(s => s.Replace("Merit Based Incentive Payment System", "Merit-Based Incentive Payment System")).ToList();
                        var codes = entity.tbl_Lookup_Registry.Select(c => c).ToList();//Codes List.
                        foreach (var item in Descriptonlist)
                        {
                            //Getting Codes from tbl_Registry table based on selected Description.
                            listarrayreg.Add(codes.Where(c => c.Description == item).Select(c => c.Code).FirstOrDefault());

                        }
                        mesNumList = (from vmRegistry in entity.vw_QPPMeasures_With_Modality
                                      where listarrayreg.Contains(vmRegistry.Registry)
                                       && mesNumList2.Contains(vmRegistry.Measure_Num)
                                      select vmRegistry.Measure_Num).Distinct().ToList();
                        strMessage += "reg: " + strModality + ":" + Convert.ToString(mesNumList.Count) + " Arraylist: " + Convert.ToString(listarrayreg.Count);

                        mesNumList2 = mesNumList;
                    }
                    // Specialty
                    if (!string.IsNullOrEmpty(strSpecialty))
                    {
                        listarrayspec = strSpecialty.Split(',').Select(i => i).ToList();
                        spec = (from m in entity.tbl_Lookup_Specialty where listarrayspec.Contains(m.Description) select m.Code).ToList();
                        mesNumList = (from vmspecialty in entity.vw_QPPMeasures_With_Modality
                                      where spec.Contains(vmspecialty.Specialty)
                                       && mesNumList2.Contains(vmspecialty.Measure_Num)
                                      select vmspecialty.Measure_Num).Distinct().ToList();

                        strMessage += "spec: " + strModality + ":" + Convert.ToString(mesNumList.Count) + " Arraylist: " + Convert.ToString(listarrayspec.Count);
                        mesNumList2 = mesNumList;
                    }
                    //Final
                    var result = entity.vw_QPPMeasures_With_Modality.Where(c => mesNumList.Contains(c.Measure_Num)).GroupBy(c => c.Measure_Num);
                    measuresList = result.Select(mes => new MeasureData
                    {
                        MeasureNumber = mes.FirstOrDefault().Measure_Num,
                        MeasureTitle = mes.FirstOrDefault().Measure_Title,
                        messageDesc = mes.FirstOrDefault().Message_Desc,
                        DisplayOrder = mes.FirstOrDefault().DisplayOrder,
                        measureTypeCode = mes.FirstOrDefault().Measure_Type_Code,
                        measure_priority = mes.FirstOrDefault().Measure_Priority,
                        Measure_URL = mes.FirstOrDefault().Measure_URL,
                        Message2 = mes.FirstOrDefault().Message2,
                        RegsitryDesc = mes.FirstOrDefault().Registry,
                        SpecialtyDesc = mes.FirstOrDefault().Specialty,
                        MopdalityDesc = mes.FirstOrDefault().Modality
                    }).OrderBy(m => m.DisplayOrder).ToList();

                }
                return measuresList;
            }
        }

        [HttpGet]
        public HttpResponseMessage GetHP(string Code)
        {
            var resu = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(Code))
                {


                    using (var entity = new DNNMIPSEntities())
                    {
                        resu = entity.tbl_Lookup_QPP_Measure_Type.Where(i => i.QPP_Measure_Type_Code == Code).Select(x => x.Description).FirstOrDefault();
                        if (resu != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, resu);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message + "\nInner exception" + Convert.ToString(ex.InnerException) + ex.StackTrace);
            }
            return Request.CreateResponse(HttpStatusCode.OK, resu);
        }

        [HttpGet]
        public HttpResponseMessage ExportToExcel(int MC, int OC, int PC, int TBP, string SM)
        {
            var data = new ExcelData();
            data.MeasuresCount = MC;
            data.OutcomeCount = OC;
            data.PriorityCount = PC;
            data.TotalBonusPoint = TBP;
            data.SelectedMeasures = SM;
            List<string> listStrings = new List<string>();
            string str = "<table border=1 style=font-family:Calibri><tr><td font style='font-weight: bold;'>Total Outcome Selected</td><td>" + data.OutcomeCount + "</td></tr><tr><td font style='font-weight: bold;'>Total High-priority Selected</td><td>" + data.PriorityCount + "</td></tr><tr><td font style='font-weight: bold;'>Total Measures Selected</td><td>" + data.MeasuresCount + "</td></tr>";
            if (data.TotalBonusPoint > 0)
            {
                str += "<tr> </tr> <tr><td colspan=7 style='color:red'>With your selections you potentially would have " + data.TotalBonusPoint + " bonus points (cap at 10% of total possible quality points; for most radiologists that is 6 points)</td></tr>";
            }
            if (data.TotalBonusPoint < 10)
            {
                str += "<tr><td colspan = 7 style='color:red'> You have not selected enough measures to meet the full requirements; to review other options please visit the QCDR page.</td></tr><tr></tr> ";
            }
            str += "<tr><td colspan=7 font style='font-weight: bold;'>Selected Measures</td></tr><tr></tr>";
            str += "<tr font style='font-weight: bold;'> <td font style='font-weight: bold;'>MeasureNumber</td><td>MeasureTitle</td><td>MeasureType</td><td>MesureTypeModifier</td><td>Registry</td><td>Domain</td><td>Speciality</td></tr>";
            using (var entity = new DNNMIPSEntities())
            {
                listStrings = data.SelectedMeasures.Split(',').ToList();

                foreach (var item in listStrings)
                {
                    var result1 = (from v in entity.vw_QPPMeasures_With_Modality
                                   where (v.Measure_Num.ToLower() == item.ToLower())
                                   select new ExportToExcelData
                                   {
                                       MeasureTitle = v.Measure_Title,
                                       MeasureType = v.MeasureType_Desc,
                                       MesureTypeModifier = v.Measure_Type_Modifier,
                                       RegistryDescription = v.registry_Desc,
                                       Domain = v.Domain,
                                       Speciality = v.specialty_Desc,
                                       MeasureNumber = v.Measure_Num
                                   }).FirstOrDefault();
                    str += "<tr> <td>" + result1.MeasureNumber + "</td><td>" + result1.MeasureTitle + "</td> <td>" + result1.MeasureType + "</td><td>" + result1.MesureTypeModifier + "</td><td>" + result1.RegistryDescription + "</td>  <td>" + result1.Domain + "</td> <td>" + result1.Speciality + "</td> </tr>";
                }
                str += "</table>";
            }
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(str);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.xls");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            result.Content.Headers.ContentDisposition.FileName = "Measuredata.xls";
            return result;
        }

        [HttpGet]
        public HttpResponseMessage ExportToExcelActivities(int HWC, int MWC, int TMC, int TBP, string SM)
        {
            var data = new ExcelData();
            data.HighweightedCount = HWC;
            data.MediumweightedCount = MWC;
            data.MeasuresCount = TMC;
            data.TotalBonusPoint = TBP;
            data.SelectedMeasures = SM;
            List<string> listStrings = new List<string>();
            string str = "<table border=1 style=font-family:Calibri><tr><td font style='font-weight: bold;'>High Weighted Activities</td><td>" + data.HighweightedCount + "</td></tr><tr><td font style='font-weight: bold;'>Medium Weighted Activities</td><td>" + data.MeasuresCount + "</td></tr><tr><td font style='font-weight: bold;'>Total Activities</td><td>" + data.MeasuresCount + "</td></tr>";
            //if (data.TotalBonusPoint > 0)
            //{
            //    str += "<tr> </tr> <tr><td colspan=7 style='color:red'>With your selections you potentially would have " + data.TotalBonusPoint + " bonus points (cap at 10% of total possible quality points; for most radiologists that is 6 points)</td></tr>";
            //}
            //if (data.TotalBonusPoint < 10)
            //{
            //    str += "<tr><td colspan = 7 style='color:red'> You have not selected enough measures to meet the full requirements; to review other options please visit the QCDR page.</td></tr><tr></tr> ";
            //}
            str += "<tr><td colspan=4 font style='font-weight: bold;'>Selected Activities</td></tr><tr></tr>";
            str += "<tr font style='font-weight: bold;'> <td font style='font-weight: bold;'>Activity ID</td><td>ActivityName</td><td>Weighing</td><td>SubCategory</td></tr>";
            using (var entity = new DNNMIPSEntities())
            {
                listStrings = data.SelectedMeasures.Split(',').ToList();

                foreach (var item in listStrings)
                {
                    var ActivityID = item.Replace(' ', '_');
                    var result1 = (from v in entity.Tbl_IA_Data
                                   where (v.ActivityID == ActivityID)
                                   select new ExportToExcelData
                                   {

                                       ActivityID = v.ActivityID,
                                       ActivityName = v.ActivityName,
                                       SubcategoryName = (entity.Tbl_lookup_ImprovementActivities.Where(d => d.Id == v.Subcategory).Select(d => d.Description).FirstOrDefault()),
                                       Weighing = v.Weighing,
                                   }).FirstOrDefault();

                    str += "<tr> <td>" + result1.ActivityID + "</td><td>" + result1.ActivityName + "</td> <td>" + result1.Weighing + "</td><td>" + result1.SubcategoryName + "</td></tr>";
                }
                str += "</table>";
            }
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(str);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.xls");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            result.Content.Headers.ContentDisposition.FileName = "Measuredata.xls";
            return result;
        }

        public List<IAData> GetActivities()
        {
            using (var entity = new DNNMIPSEntities())
            {
                var result = entity.Tbl_lookup_ImprovementActivities.Select(c =>
                new IAData
                {
                    Id = c.Id,
                    SubcategoryName = c.Description
                }).ToList<IAData>();
                return result;
            };
        }
        public List<IAData> GetActivitiesList(string Sc, string Wt)
        {
            using (var entity = new DNNMIPSEntities())
            {
                var result = new List<IAData>();


                if (Wt == null)
                {
                    List<int> SubcategoryItems = Sc.Split(',').Select(i => int.Parse(i)).ToList();
                    result = (from c in entity.Tbl_IA_Data
                              where SubcategoryItems.Contains(c.Subcategory)
                              select
                                    new IAData
                                    {
                                        ActivityID = c.ActivityID,
                                        ActivityDescription = c.ActivityDescription,
                                        Weighing = c.Weighing,
                                        ActivityName = c.ActivityName,
                                        Validations = c.Validations,
                                        SubcategoryName = (entity.Tbl_lookup_ImprovementActivities.Where(d => d.Id == c.Subcategory).Select(d => d.Description).FirstOrDefault()),
                                        CMSsuggesteddocuments = c.CMSsuggesteddocuments,
                                        ACRsuggesteddocuments = c.ACRsuggesteddocuments,
                                        Message = c.Message == null ? "" : c.Message
                                    }).ToList();

                }
                else if (Sc == null)
                {
                    List<string> WeighsList = Wt.Split(',').Select(i => i).ToList();
                    result = (from c in entity.Tbl_IA_Data
                              where WeighsList.Contains(c.Weighing)
                              select
                                    new IAData
                                    {
                                        ActivityID = c.ActivityID,
                                        ActivityDescription = c.ActivityDescription,
                                        Weighing = c.Weighing,
                                        ActivityName = c.ActivityName,
                                        Validations = c.Validations,
                                        SubcategoryName = (entity.Tbl_lookup_ImprovementActivities.Where(d => d.Id == c.Subcategory).Select(d => d.Description).FirstOrDefault()),
                                        CMSsuggesteddocuments = c.CMSsuggesteddocuments,
                                        ACRsuggesteddocuments = c.ACRsuggesteddocuments,
                                        Message = c.Message == null ? "" : c.Message
                                    }).ToList();
                }
                else
                {
                    List<int> SubcategoryItems = Sc.Split(',').Select(i => int.Parse(i)).ToList();
                    List<string> WeighsList = Wt.Split(',').Select(i => i).ToList();
                    result = (from c in entity.Tbl_IA_Data
                              where SubcategoryItems.Contains(c.Subcategory) && WeighsList.Contains(c.Weighing)
                              select
                                new IAData
                                {
                                    ActivityID = c.ActivityID,
                                    ActivityDescription = c.ActivityDescription,
                                    Weighing = c.Weighing,
                                    ActivityName = c.ActivityName,
                                    Validations = c.Validations,
                                    SubcategoryName = (entity.Tbl_lookup_ImprovementActivities.Where(d => d.Id == c.Subcategory).Select(d => d.Description).FirstOrDefault()),
                                    CMSsuggesteddocuments = c.CMSsuggesteddocuments,
                                    ACRsuggesteddocuments = c.ACRsuggesteddocuments,
                                    Message = c.Message == null ? "" : c.Message
                                }).ToList();


                }
                return result;

            };
        }
        public List<IAData> GetAllActivitiesList()
        {
            using (var entity = new DNNMIPSEntities())
            {
                var result = entity.Tbl_IA_Data.Select(c =>
                      new IAData
                      {
                          ActivityID = c.ActivityID,
                          ActivityDescription = c.ActivityDescription,
                          Weighing = c.Weighing,
                          SubcategoryName = (entity.Tbl_lookup_ImprovementActivities.Where(d => d.Id == c.Subcategory).Select(d => d.Description).FirstOrDefault()),
                          ActivityName = c.ActivityName,
                          Validations = c.Validations,
                          CMSsuggesteddocuments = c.CMSsuggesteddocuments,
                          ACRsuggesteddocuments = c.ACRsuggesteddocuments,
                          Message = c.Message == null ? "" : c.Message
                      }).ToList();
                return result;
            };
        }

    }
}