using ClosedXML.Excel;
using DNNAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Data;
using System.IO;
using System.Web;

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

                MeasureDataList = (from mes in entity.vw_QPPMeasures_With_Modality

                                   select new MeasureData
                                   {
                                       MeasureNumber = mes.Measure_Num,
                                       MeasureTitle = mes.Measure_Title,
                                       messageDesc = mes.Measure_Description,
                                       DisplayOrder = mes.DisplayOrder,
                                       MeasureTypeDesc = mes.Message_Desc,
                                       MopdalityDesc = mes.Modality,
                                       RegsitryDesc = mes.Registry,
                                       SpecialtyDesc = mes.specialty_Desc,
                                       measureTypeCode = mes.Measure_Type_Code,
                                       measure_priority = mes.Measure_Priority,
                                       Measure_URL = mes.Measure_URL,
                                       Message2 = mes.Message2

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
        public List<MeasureData> GetAllSelectedMeasures(string arrayOfValues) //,string[] chkModality,string[] chkRegistry,string chkSpeciality)
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
        //public List<vw_QPPMeasures_With_Modality> GetSelectionMeasures(string strMeasureType, string strModality = "", string strRegistry = "", string strSpecialty = "")
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

                    measuresList = (from mes in entity.vw_QPPMeasures_With_Modality
                                    where (mesNumList.Contains(mes.Measure_Num))
                                    select new MeasureData
                                    {
                                        MeasureNumber = mes.Measure_Num,
                                        MeasureTitle = mes.Measure_Title,
                                        messageDesc = mes.Message_Desc,
                                        DisplayOrder = mes.DisplayOrder,
                                        measureTypeCode = mes.Measure_Type_Code,
                                        measure_priority = mes.Measure_Priority,
                                        Measure_URL = mes.Measure_URL,
                                        Message2 = mes.Message2
                                    }).OrderBy(m => m.DisplayOrder).ToList();

                }
                //var x = new MeasureData();
                //x.MeasureNumber = strMeasureType + " [Total Result: " + Convert.ToString(measuresList.Count) + "List Array" + Convert.ToString(listarray.Count());
                //x.MeasureTitle = string.Join(",", mtc.ToArray()) + strMessage;
                //x.DisplayOrder = Int32.Parse("99999");
                //measuresList.Add(x);
            }
            return measuresList;
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

        public MemoryStream GetStream(XLWorkbook excelWorkbook)
        {
            MemoryStream fs = new MemoryStream();
            excelWorkbook.SaveAs(fs);
            fs.Position = 0;
            return fs;
        }
        [HttpGet]
        //public List<ExportToExcelData> ExportToExcel(int MC, int OC, int PC, int TBP, string SM)
        public List<ExportToExcelData> ExportToExcel(int MC, int OC, int PC, int TBP, string SM)
        {
            var data = new ExcelData();
            data.MeasuresCount = MC;
            data.OutcomeCoumt = OC;
            data.PriorityCount = PC;
            data.TotalBonusPoint = TBP;
            data.SelectedMeasures = SM;
            List<string> listStrings = new List<string>();
            List<ExportToExcelData> obj = new List<ExportToExcelData>();
            string str = "<table border=1 style=font-family:Calibri><tr><td font style='font-weight: bold;'>Total Outcome Selected</td><td>" + data.OutcomeCoumt + "</td></tr><tr><td font style='font-weight: bold;'>Total High-priority Selected</td><td>" + data.PriorityCount + "</td></tr><tr><td font style='font-weight: bold;'>Total Measures Selected</td><td>" + data.MeasuresCount + "</td></tr>";
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
                    obj.Add(result1);
                }
                str += "</table>";
            }
            //HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            //result.Content = new StringContent(str);
            //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.xls");
            //result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            //result.Content.Headers.ContentDisposition.FileName = "Measuredata.xls";
            //return result;
            return obj;

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
                                        Validations = "",
                                        SubcategoryName = (entity.Tbl_lookup_ImprovementActivities.Where(d => d.Id == c.Subcategory).Select(d => d.Description).FirstOrDefault()),
                                        CMSsuggesteddocuments = "",
                                        ACRsuggesteddocuments = ""
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
                                        Validations = "",
                                        SubcategoryName = (entity.Tbl_lookup_ImprovementActivities.Where(d => d.Id == c.Subcategory).Select(d => d.Description).FirstOrDefault()),
                                        CMSsuggesteddocuments = "",
                                        ACRsuggesteddocuments = ""
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
                                    Validations = "",
                                    SubcategoryName = (entity.Tbl_lookup_ImprovementActivities.Where(d => d.Id == c.Subcategory).Select(d => d.Description).FirstOrDefault()),
                                    CMSsuggesteddocuments = "",
                                    ACRsuggesteddocuments = ""
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
                          Validations = "",
                          CMSsuggesteddocuments = "",
                          ACRsuggesteddocuments = ""
                      }).ToList();
                return result;
            };
        }

    }
}