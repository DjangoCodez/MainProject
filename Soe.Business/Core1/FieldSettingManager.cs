using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class FieldSettingManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static readonly object fieldsLock = new object();
        static readonly object formsLock = new object();

        #endregion

        #region Ctor

        public FieldSettingManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region SettingObject

        /// <summary>
        /// Load all RoleFieldSettings and CompanyFieldSettings for a given Form.
        /// Sorts the collection on RoleFieldSettings (highest prio)
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="formId">The FormId</param>
        /// <param name="roleId">The RoleId</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>A collection of Setting entities for the given Form</returns>
        public List<SettingObject> GetSettingsForForm(int formId, int? roleId, int actorCompanyId)
        {
            List<SettingObject> settings = new List<SettingObject>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            //Get RoleFieldSettings
            entitiesReadOnly.RoleFieldSetting.NoTracking();
            settings.AddRange(from fs in entitiesReadOnly.RoleFieldSetting
                              where fs.FormId == formId &&
                              (!roleId.HasValue || fs.RoleId == roleId.Value)
                              select new SettingObject
                              {
                                  RoleId = fs.RoleId,
                                  ActorCompanyId = null,
                                  FormId = fs.Form.FormId,
                                  FormName = fs.Form.Name,
                                  FieldId = fs.Field.FieldId,
                                  FieldName = fs.Field.Name,
                                  SysSettingId = fs.SysSettingId,
                                  Value = fs.Value,
                                  Type = (SoeFieldSettingType)fs.Type,
                              });

            //Get CompanyFieldSettings
            entitiesReadOnly.CompanyFieldSetting.NoTracking();
            settings.AddRange(from fs in entitiesReadOnly.CompanyFieldSetting
                              where fs.FormId == formId &&
                              fs.ActorCompanyId == actorCompanyId
                              select new SettingObject
                              {
                                  RoleId = null,
                                  ActorCompanyId = fs.ActorCompanyId,
                                  FormId = fs.Form.FormId,
                                  FormName = fs.Form.Name,
                                  FieldId = fs.Field.FieldId,
                                  FieldName = fs.Field.Name,
                                  SysSettingId = fs.SysSettingId,
                                  Value = fs.Value,
                                  Type = (SoeFieldSettingType)fs.Type,
                              });

            return settings;
        }

        #endregion

        #region FieldSettingDTO

        public List<FieldSettingDTO> GetFieldsAndSettings(SoeFieldSettingType type, int actorCompanyId, int? fieldId = null)
        {
            List<FieldSettingDTO> dtos = new List<FieldSettingDTO>();

            List<Role> roles = RoleManager.GetRolesByCompany(actorCompanyId);
            List<Form> forms = GetForms(type);

            foreach (Form form in forms.OrderBy(i => i.Name))
            {
                List<SettingObject> settingsForForm = GetSettingsForForm(form.FormId, null, actorCompanyId);
                List<Field> fields = GetFields(form.FormId);

                if (fieldId.HasValue)
                    fields = fields.Where(x => x.FieldId == fieldId).ToList();

                foreach (Field field in fields.OrderBy(i => i.Name))
                {
                    #region Limitations

                    bool supported = true;

                    int sysTermId = 0;
                    if (int.TryParse(field.Name, out sysTermId))
                    {
                        //Not supported fields
                        switch (sysTermId)
                        {
                            case (int)TermGroup_MobileFields.OrderGrid_VatType:
                            case (int)TermGroup_MobileFields.OrderGrid_SalesPriceList:
                            case (int)TermGroup_MobileFields.OrderGrid_Label:
                            case (int)TermGroup_MobileFields.OrderGrid_InvoiceAddress:
                            case (int)TermGroup_MobileFields.OrderGrid_Reference:
                            case (int)TermGroup_MobileFields.OrderGrid_HeadText:
                                supported = false;
                                break;

                            case (int)TermGroup_MobileFields.CustomerGrid_VatNr:
                            case (int)TermGroup_MobileFields.CustomerGrid_EmailAddress:
                            case (int)TermGroup_MobileFields.CustomerGrid_PhoneHome:
                            case (int)TermGroup_MobileFields.CustomerGrid_PhoneJob:
                            case (int)TermGroup_MobileFields.CustomerGrid_PhoneMobile:
                            case (int)TermGroup_MobileFields.CustomerGrid_Fax:
                            //case (int)TermGroup_MobileFields.CustomerGrid_InvoiceAddress:
                            //case (int)TermGroup_MobileFields.CustomerGrid_DeliveryAddress1:
                            case (int)TermGroup_MobileFields.CustomerGrid_VatType:
                            case (int)TermGroup_MobileFields.CustomerGrid_PaymentCondition:
                            case (int)TermGroup_MobileFields.CustomerGrid_SalesPriceList:
                            case (int)TermGroup_MobileFields.CustomerGrid_StandardWholeSeller:
                            case (int)TermGroup_MobileFields.CustomerGrid_DiscountArticles:
                            case (int)TermGroup_MobileFields.CustomerGrid_DiscountServices:
                            case (int)TermGroup_MobileFields.CustomerGrid_Currency:
                            case (int)TermGroup_MobileFields.CustomerGrid_Note:
                                supported = false;
                                break;

                            case (int)TermGroup_MobileFields.OrderEdit_HeadText:
                                supported = false;
                                break;

                        }

                        if (!supported)
                            continue;
                    }

                    #endregion

                    #region Field

                    string formName = form.Name;
                    if (Int32.TryParse(formName, out sysTermId) && sysTermId > 0)
                        formName = GetText(sysTermId, (int)TermGroup.MobileForms);
                    string fieldName = field.Name;
                    if (Int32.TryParse(fieldName, out sysTermId) && sysTermId > 0)
                        fieldName = GetText(sysTermId, (int)TermGroup.MobileFields);

                    FieldSettingDTO dto = new FieldSettingDTO()
                    {
                        FormId = form.FormId,
                        FormName = formName,
                        FieldId = field.FieldId,
                        FieldName = fieldName,
                        Type = type,
                    };

                    //CompanySetting
                    dto.CompanySetting = new CompanyFieldSettingDTO()
                    {
                        ActorCompanyId = actorCompanyId,
                    };

                    //RoleSettings
                    dto.RoleSettings = new List<RoleFieldSettingDTO>();
                    foreach (Role role in roles)
                    {
                        dto.RoleSettings.Add(new RoleFieldSettingDTO()
                        {
                            RoleId = role.RoleId,
                            RoleName = role.Name,
                        });
                    }

                    List<SettingObject> settingsForField = settingsForForm.Where(i => i.FieldId == field.FieldId).ToList();
                    foreach (SettingObject setting in settingsForField)
                    {
                        #region SettingObject

                        ISoeSetting soeSetting = null;
                        if (setting.ActorCompanyId.HasValue)
                            soeSetting = dto.CompanySetting;
                        else if (setting.RoleId.HasValue)
                            soeSetting = dto.RoleSettings.FirstOrDefault(i => i.RoleId == setting.RoleId.Value);

                        if (soeSetting != null)
                        {
                            switch (setting.SysSettingId)
                            {
                                case (int)SoeSetting.Label:
                                    soeSetting.Label = setting.Value;
                                    break;
                                case (int)SoeSetting.Visible:
                                    soeSetting.Visible = StringUtility.GetBool(setting.Value);
                                    break;
                                case (int)SoeSetting.SkipTabStop:
                                    soeSetting.SkipTabStop = StringUtility.GetBool(setting.Value);
                                    break;
                                case (int)SoeSetting.ReadOnly:
                                    soeSetting.ReadOnly = StringUtility.GetBool(setting.Value);
                                    break;
                                case (int)SoeSetting.BoldLabel:
                                    soeSetting.BoldLabel = StringUtility.GetBool(setting.Value);
                                    break;
                            }
                        }

                        #endregion
                    }

                    #region Summary

                    if (dto.CompanySetting.Visible.HasValue)
                    {
                        string settingText = dto.CompanySetting.Visible.Value ? GetText(5713, "Ja") : GetText(5714, "Nej");
                        dto.CompanySettingsSummary = $"{GetText(5712, "Synligt")} - {settingText}";
                    }

                    List<RoleFieldSettingDTO> roleSettings = dto.RoleSettings.Where(i => i.Visible.HasValue).ToList();
                    if (roleSettings.Any())
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append($"{GetText(5712, "Synligt")} - ");

                        int counter = 1;
                        foreach (RoleFieldSettingDTO roleSetting in roleSettings.OrderBy(i => i.Visible.Value))
                        {
                            sb.Append(roleSetting.RoleName);
                            sb.Append(":");
                            if (roleSetting.Visible.HasValue)
                                sb.Append(roleSetting.Visible.Value ? GetText(5713, "Ja") : GetText(5714, "Nej"));
                            if (counter < roleSettings.Count)
                                sb.Append(",");
                            sb.Append(" ");

                            counter++;
                        }

                        dto.RoleSettingsSummary = sb.ToString();
                    }

                    #endregion

                    dtos.Add(dto);

                    #endregion
                }
            }

            return dtos.OrderBy(i => i.FormId).ThenBy(i => i.FieldId).ToList();
        }

        public ActionResult SaveFieldSettings(FieldSettingDTO dto, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            if (dto == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "FieldSettingDTO");
            if (dto.CompanySetting == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "CompanyFieldSettingDTO");
            if (dto.RoleSettings == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "RoleFieldSettingDTO");

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        string value = "";

                        #region Supported settings

                        #region Visible

                        if (dto.CompanySetting.Visible.HasValue)
                            value = dto.CompanySetting.Visible.Value.ToString();
                        else
                            value = Convert.ToString((int)TermGroup_YesNoDefault.Default, CultureInfo.InvariantCulture);

                        result = SaveCompanyFieldSetting(entities, dto.FormId, dto.FieldId, (int)SoeSetting.Visible, value, actorCompanyId);
                        if (!result.Success)
                            return result;

                        foreach (RoleFieldSettingDTO roleDTO in dto.RoleSettings)
                        {
                            if (roleDTO.Visible.HasValue)
                                value = roleDTO.Visible.Value.ToString();
                            else
                                value = Convert.ToString((int)TermGroup_YesNoDefault.Default, CultureInfo.InvariantCulture);

                            result = SaveRoleFieldSetting(entities, dto.FormId, dto.FieldId, (int)SoeSetting.Visible, value, roleDTO.RoleId);
                        }

                        #endregion

                        #endregion

                        #region Save

                        if (result.Success)
                            result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                        }

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = dto.FieldId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        #endregion

        #region FieldSetting

        /// <summary>
        /// Get the active RoleFieldSettings and CompanyFieldSettings for a given mobile form.
        /// Role settings has prio 1.
        /// Company settings has prio 2.
        /// </summary>
        /// <param name="mobileForm">The mobile Form</param>
        /// <param name="roleId">The RoleId</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>Collection of FieldSettings</returns>
        public List<FieldSetting> GetFieldSettingsForMobileForm(TermGroup_MobileForms mobileForm, int? roleId, int actorCompanyId)
        {
            Form form = GetMobileForm(mobileForm);
            if (form == null)
                return new List<FieldSetting>();

            List<FieldSetting> fieldSettings = new List<FieldSetting>();

            List<SettingObject> settings = GetSettingsForForm(form.FormId, roleId, actorCompanyId);
            List<Field> fields = GetFields(form.FormId);

            foreach (Field field in fields)
            {
                FieldSetting fieldSetting = FilterFieldSetting(field, settings);
                if (fieldSetting != null)
                    fieldSettings.Add(fieldSetting);
            }

            return fieldSettings;
        }

        /// <summary>
        /// Get the active FieldSetting for a given Field and Role
        /// </summary>
        /// <param name="formId">The FormId</param>
        /// <param name="fieldId">The FieldId</param>
        /// <param name="roleId">The RoleId</param>
        /// <returns>A FieldSetting with aggregated fieldsetting information</returns>
        public FieldSetting GetFieldSettingForRole(int formId, int fieldId, int roleId)
        {
            Field field = GetField(fieldId, formId);
            if (field == null)
                return null;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.RoleFieldSetting.NoTracking();
            var roleFieldSettings = (from fs in entities.RoleFieldSetting
                                     where fs.Role.RoleId == roleId &&
                                     fs.Form.FormId == formId &&
                                     fs.Field.FieldId == fieldId
                                     select new SettingObject
                                     {
                                         RoleId = fs.RoleId,
                                         ActorCompanyId = null,
                                         FormId = fs.Form.FormId,
                                         FormName = fs.Form.Name,
                                         FieldId = fs.Field.FieldId,
                                         FieldName = fs.Field.Name,
                                         SysSettingId = fs.SysSettingId,
                                         Value = fs.Value,
                                         Type = (SoeFieldSettingType)fs.Type,
                                     }).ToList();

            return FilterFieldSetting(field, roleFieldSettings);
        }

        /// <summary>
        /// Get the active FieldSetting for a given Field and Company
        /// </summary>
        /// <param name="formId">The FormId</param>
        /// <param name="fieldId">The FieldId</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>A FieldSetting with aggregated fieldsetting information</returns>
        public FieldSetting GetFieldSettingForCompany(int formId, int fieldId, int actorCompanyId)
        {
            Field field = GetField(fieldId, formId);
            if (field == null)
                return null;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyFieldSetting.NoTracking();
            var companyFieldSettings = (from fs in entities.CompanyFieldSetting
                                        where fs.Company.ActorCompanyId == actorCompanyId &&
                                        fs.Form.FormId == formId &&
                                        fs.Field.FieldId == fieldId
                                        select new SettingObject
                                        {
                                            RoleId = null,
                                            ActorCompanyId = fs.ActorCompanyId,
                                            FormId = fs.Form.FormId,
                                            FormName = fs.Form.Name,
                                            FieldId = fs.Field.FieldId,
                                            FieldName = fs.Field.Name,
                                            SysSettingId = fs.SysSettingId,
                                            Value = fs.Value,
                                            Type = (SoeFieldSettingType)fs.Type,
                                        }).ToList();

            return FilterFieldSetting(field, companyFieldSettings);
        }

        /// <summary>
        /// Get the active RoleFieldSettings and CompanyFieldSettings for a given Field.
        /// Role settings has prio 1.
        /// Company settings has prio 2.
        /// </summary>
        /// <param name="field">The Field	</param>
        /// <param name="settings">All Settings for the Fields in the Form</param>
        /// <returns>A FieldSetting entity for the given Field</returns>
        public FieldSetting FilterFieldSetting(Field field, List<SettingObject> settings)
        {
            if (field == null || settings == null)
                return null;

            FieldSetting fieldSetting = null;

            //Get settings for given field. Orderby RoleId so null values (i.e. CompanyFieldSettings) returns last
            List<SettingObject> settingsForField = (from fs in settings
                                                    where fs.FieldId == field.FieldId
                                                    orderby fs.RoleId descending
                                                    select fs).ToList();

            if (settingsForField != null && settingsForField.Count > 0)
            {
                SettingObject firstSettingForField = settingsForField.FirstOrDefault();

                //Create FieldSetting
                fieldSetting = new FieldSetting()
                {
                    RoleId = firstSettingForField.RoleId,
                    ActorCompanyId = firstSettingForField.ActorCompanyId,
                    FormId = firstSettingForField.FormId,
                    FormName = firstSettingForField.FormName,
                    FieldId = Convert.ToInt32(firstSettingForField.FieldId, CultureInfo.InvariantCulture),
                    FieldName = firstSettingForField.FieldName,
                };

                foreach (SettingObject settingForField in settingsForField)
                {
                    //Add the FieldSettingDetail to the FieldSetting
                    fieldSetting.AddFieldSettingDetail(new FieldSettingDetail()
                    {
                        SysSettingId = settingForField.SysSettingId,
                        Value = settingForField.Value,
                    });
                }
            }

            return fieldSetting;
        }

        public List<FieldSetting> GetFieldsSettingsForField(List<FieldSetting> fieldSettings, TermGroup_MobileFields mobileField)
        {
            if (fieldSettings == null)
                return new List<FieldSetting>();

            //Get settings for given field. Orderby RoleId so null values (i.e. CompanyFieldSettings) returns last
            return (from fs in fieldSettings
                    where fs.FieldName == ((int)mobileField).ToString()
                    select fs).OrderByDescending(i => i.RoleId).ToList();
        }

        public bool DoShowMobileField(List<FieldSetting> fieldSettings, TermGroup_MobileFields mobileField, bool defaultValue = true, bool showAllFields = false)
        {
            if (showAllFields)
                return true;

            bool doShowField = defaultValue;

            List<FieldSetting> fieldSettingsForField = GetFieldsSettingsForField(fieldSettings, mobileField);
            foreach (FieldSetting fieldSetting in fieldSettingsForField)
            {
                foreach (FieldSettingDetail fieldSettingDetail in fieldSetting.GetFieldSettingDetails())
                {
                    if (fieldSettingDetail.SysSettingId == (int)SoeSetting.Visible)
                        doShowField = StringUtility.GetBool(fieldSettingDetail.Value);
                }
            }

            return doShowField;
        }

        #endregion

        #region RoleFieldSetting

        /// <summary>
        /// Get the RoleFieldSettings for a given Field.
        /// </summary>
        /// <param name="roleId">The RoleId</param>
        /// <param name="formName">The FormId</param>
        /// <param name="fieldName">The FieldId</param>
        /// <returns>A RoleFieldSetting entity</returns>
        public RoleFieldSetting GetRoleFieldSetting(int fieldId, int formId, int roleId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.RoleFieldSetting.NoTracking();
            return GetRoleFieldSetting(entities, fieldId, formId, roleId);
        }

        /// <summary>
        /// Get the RoleFieldSettings for a given Field.
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="fieldId">The FieldId</param>
        /// <param name="formId">The FormId</param>
        /// <param name="roleId">The RoleId</param>
        /// <returns>A RoleFieldSetting entity</returns>
        public RoleFieldSetting GetRoleFieldSetting(CompEntities entities, int fieldId, int formId, int roleId)
        {
            return (from rfs in entities.RoleFieldSetting
                    where rfs.Role.RoleId == roleId &&
                    rfs.Form.FormId == formId &&
                    rfs.Field.FieldId == fieldId
                    select rfs).FirstOrDefault();
        }

        /// <summary>
        /// Get the RoleFieldSettings for a given Field.
        /// </summary>
        /// <param name="entities">The Object Contect</param>
        /// <param name="fieldId">The FieldId</param>
        /// <param name="formId">The FormId</param>
        /// <param name="sysSettingId">The SysSetting</param>
        /// <param name="roleId">The RoleId</param>
        /// <returns>A RoleFieldSetting entity</returns>
        public RoleFieldSetting GetRoleFieldSetting(int fieldId, int formId, int sysSettingId, int roleId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.RoleFieldSetting.NoTracking();
            return GetRoleFieldSetting(entities, fieldId, formId, sysSettingId, roleId);
        }

        /// <summary>
        /// Get the RoleFieldSettings for a given Field and SysSetting.
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="fieldId">The FieldId</param>
        /// <param name="formId">The FormId</param>
        /// <param name="sysSettingId">The SysSetting</param>
        /// <param name="roleId">The RoleId</param>
        /// <returns>A RoleFieldSetting entity</returns>
        public RoleFieldSetting GetRoleFieldSetting(CompEntities entities, int fieldId, int formId, int sysSettingId, int roleId)
        {
            return (from rfs in entities.RoleFieldSetting
                    where rfs.Role.RoleId == roleId &&
                    rfs.Form.FormId == formId &&
                    rfs.Field.FieldId == fieldId &&
                    rfs.SysSettingId == sysSettingId
                    select rfs).FirstOrDefault();
        }

        /// <summary>
        /// Save a RoleFieldSetting.
        /// Delete if it exists and value not is null or empty.
        /// Updated if it exists and value not is null or empty.
        /// Added if it not exists and value not is null or empty.
        /// </summary>
        /// <param name="formId">The FormId</param>
        /// <param name="fieldId">The FieldId</param>
        /// <param name="sysSettingId">The SysSettingId</param>
        /// <param name="value">The value for the RoleFieldSetting</param>
        /// <param name="roleId">The RoleId</param>
        /// <returns>ActionResult</returns>
        public ActionResult SaveRoleFieldSetting(int formId, int fieldId, int sysSettingId, string value, int roleId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return SaveRoleFieldSetting(entities, formId, fieldId, sysSettingId, value, roleId);
            }
        }

        /// <summary>
        /// Save a RoleFieldSetting.
        /// Delete if it exists and value not is null or empty.
        /// Updated if it exists and value not is null or empty.
        /// Added if it not exists and value not is null or empty.
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="formId">The FormId</param>
        /// <param name="fieldId">The FieldId</param>
        /// <param name="sysSettingId">The SysSettingId</param>
        /// <param name="value">The value for the RoleFieldSetting</param>
        /// <param name="roleId">The RoleId</param>
        /// <returns>ActionResult</returns>
        public ActionResult SaveRoleFieldSetting(CompEntities entities, int formId, int fieldId, int sysSettingId, string value, int roleId)
        {
            ActionResult result = new ActionResult(true);

            bool hasDefaultValue = sysSettingId == (int)SoeSetting.Visible || sysSettingId == (int)SoeSetting.SkipTabStop || sysSettingId == (int)SoeSetting.ReadOnly || sysSettingId == (int)SoeSetting.BoldLabel;
            bool isDefaultValue = hasDefaultValue && value == Convert.ToString((int)TermGroup_YesNoDefault.Default, CultureInfo.InvariantCulture);

            RoleFieldSetting roleFieldSetting = GetRoleFieldSetting(entities, fieldId, formId, sysSettingId, roleId);
            if (roleFieldSetting != null)
            {
                #region Update

                if (String.IsNullOrEmpty(value))
                {
                    //Delete (restore to default)
                    return DeleteEntityItem(entities, roleFieldSetting);
                }

                if (roleFieldSetting.Value == value)
                    return new ActionResult(true);

                //SelectEntry with default value - Delete (restore to default)
                if (isDefaultValue)
                    return DeleteEntityItem(entities, roleFieldSetting);

                //Update
                roleFieldSetting.Value = value;

                result = SaveEntityItem(entities, roleFieldSetting);

                #endregion
            }
            else
            {
                #region Add

                if (String.IsNullOrEmpty(value))
                    return new ActionResult(true);

                //Special cases:
                bool add = true;

                //SelectEntry with default value - do not add
                if (isDefaultValue)
                    add = false;

                if (add)
                {
                    roleFieldSetting = new RoleFieldSetting()
                    {
                        SysSettingId = sysSettingId,
                        Value = value,

                        //Set FK
                        RoleId = roleId,
                        FormId = formId,
                        FieldId = fieldId,
                    };

                    result = AddEntityItem(entities, roleFieldSetting, "RoleFieldSetting");
                }

                #endregion
            }

            return result;
        }

        /// <summary>
        /// Sets a RoleFieldSetting to Deleted
        /// </summary>
        /// <param name="report">RoleFieldSetting to delete</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteRoleFieldSetting(RoleFieldSetting roleFieldSetting)
        {
            if (roleFieldSetting == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "RoleFieldSetting");

            using (CompEntities entities = new CompEntities())
            {
                RoleFieldSetting originalRoleFieldSetting = GetRoleFieldSetting(entities, roleFieldSetting.FieldId, roleFieldSetting.FormId, roleFieldSetting.RoleId);
                if (originalRoleFieldSetting == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "RoleFieldSetting");

                return DeleteEntityItem(entities, originalRoleFieldSetting);
            }
        }

        #endregion

        #region CompanyFieldSetting

        /// <summary>
        /// Get all CompanyFieldSettings for a given Company
        /// </summary>
        /// <param name="actorCompanyId"></param>
        /// <returns></returns>
        public List<CompanyFieldSetting> GetCompanyFieldSettings(int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from cfs in entitiesReadOnly.CompanyFieldSetting
                    where cfs.ActorCompanyId == actorCompanyId
                    select cfs).ToList();
        }

        /// <summary>
        /// Get the CompanyFieldSettings for a given Field.
        /// </summary>
        /// <param name="fieldId">The FieldId</param>
        /// <param name="formId">The FormId</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>A CompanyFieldSetting entity</returns>
        public CompanyFieldSetting GetCompanyFieldSetting(int fieldId, int formId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyFieldSetting.NoTracking();
            return GetCompanyFieldSetting(entities, fieldId, formId, actorCompanyId);
        }

        /// <summary>
        /// Get the CompanyFieldSettings for a given Field.
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="fieldId">The FieldId</param>
        /// <param name="formId">The FormId</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>A CompanyFieldSetting entity</returns>
        public CompanyFieldSetting GetCompanyFieldSetting(CompEntities entities, int fieldId, int formId, int actorCompanyId)
        {
            return (from cfs in entities.CompanyFieldSetting
                    where cfs.Company.ActorCompanyId == actorCompanyId &&
                    cfs.Form.FormId == formId &&
                    cfs.Field.FieldId == fieldId
                    select cfs).FirstOrDefault();
        }

        /// <summary>
        /// Get the CompanyFieldSettings for a given Field.
        /// </summary>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <param name="fieldId">The FieldId</param>
        /// <param name="formId">The FormId</param>
        /// <returns>A CompanyFieldSetting entity</returns>
        public CompanyFieldSetting GetCompanyFieldSetting(int fieldId, int formId, int sysSettingId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyFieldSetting.NoTracking();
            return GetCompanyFieldSetting(entities, fieldId, formId, sysSettingId, actorCompanyId);
        }

        /// <summary>
        /// Get the CompanyFieldSettings for a given Field and SysSetting
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="fieldId">The FieldId</param>
        /// <param name="formId">The FormId</param>
        /// <param name="sysSettingId">The SysSetting</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>A CompanyFieldSetting entity</returns>
        public CompanyFieldSetting GetCompanyFieldSetting(CompEntities entities, int fieldId, int formId, int sysSettingId, int actorCompanyId)
        {
            return (from cfs in entities.CompanyFieldSetting
                    where cfs.Company.ActorCompanyId == actorCompanyId &&
                    cfs.Form.FormId == formId &&
                    cfs.Field.FieldId == fieldId &&
                    cfs.SysSettingId == sysSettingId
                    select cfs).FirstOrDefault();
        }

        /// <summary>
        /// Save a CompanyFieldSetting.
        /// Delete if it exists and value not is null or empty.
        /// Updated if it exists and value not is null or empty.
        /// Added if it not exists and value not is null or empty.
        /// </summary>
        /// <param name="formId">The FormId</param>
        /// <param name="fieldId">The FieldId</param>
        /// <param name="sysSettingId">The SysSettingId</param>
        /// <param name="value">The value for the CompanyFieldSetting</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult SaveCompanyFieldSetting(int formId, int fieldId, int sysSettingId, string value, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return SaveCompanyFieldSetting(entities, formId, fieldId, sysSettingId, value, actorCompanyId);
            }
        }

        /// <summary>
        /// Save a CompanyFieldSetting.
        /// Delete if it exists and value not is null or empty.
        /// Updated if it exists and value not is null or empty.
        /// Added if it not exists and value not is null or empty.
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="formId">The FormId</param>
        /// <param name="fieldId">The FieldId</param>
        /// <param name="sysSettingId">The SysSettingId</param>
        /// <param name="value">The value for the CompanyFieldSetting</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult SaveCompanyFieldSetting(CompEntities entities, int formId, int fieldId, int sysSettingId, string value, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            bool hasDefaultValue = sysSettingId == (int)SoeSetting.Visible || sysSettingId == (int)SoeSetting.SkipTabStop || sysSettingId == (int)SoeSetting.ReadOnly || sysSettingId == (int)SoeSetting.BoldLabel;
            bool isDefaultValue = hasDefaultValue && value == Convert.ToString((int)TermGroup_YesNoDefault.Default, CultureInfo.InvariantCulture);

            CompanyFieldSetting companyFieldSetting = GetCompanyFieldSetting(entities, fieldId, formId, sysSettingId, actorCompanyId);
            if (companyFieldSetting != null)
            {
                #region Update

                //Delete (restore to default)
                if (String.IsNullOrEmpty(value))
                    return DeleteEntityItem(entities, companyFieldSetting);

                if (companyFieldSetting.Value == value)
                    return new ActionResult(true);

                //SelectEntry with default value - Delete (restore to default)
                if (isDefaultValue)
                    return DeleteEntityItem(entities, companyFieldSetting);

                //Update
                companyFieldSetting.Value = value;

                result = SaveEntityItem(entities, companyFieldSetting);

                #endregion
            }
            else
            {
                #region Add

                if (String.IsNullOrEmpty(value))
                    return new ActionResult(true);

                //Special cases:
                bool add = true;

                //SelectEntry with default value - do not add
                if (isDefaultValue)
                    add = false;

                if (add)
                {
                    companyFieldSetting = new CompanyFieldSetting()
                    {
                        SysSettingId = sysSettingId,
                        Value = value,

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        FieldId = fieldId,
                        FormId = formId,
                    };

                    result = AddEntityItem(entities, companyFieldSetting, "CompanyFieldSetting");
                }

                #endregion
            }

            return result;
        }

        /// <summary>
        /// Sets a CompanyFieldSetting to Deleted
        /// </summary>
        /// <param name="report">CompanyFieldSetting to delete</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteCompanyFieldSetting(CompanyFieldSetting companyFieldSetting)
        {
            if (companyFieldSetting == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "CompanyFieldSetting");

            using (CompEntities entities = new CompEntities())
            {
                CompanyFieldSetting originalCompanyFieldSetting = GetCompanyFieldSetting(entities, companyFieldSetting.FieldId, companyFieldSetting.FormId, companyFieldSetting.ActorCompanyId);
                if (originalCompanyFieldSetting == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "CompanyFieldSetting");

                return DeleteEntityItem(entities, originalCompanyFieldSetting);
            }
        }

        #endregion

        #region Form

        #region From DB

        public List<Form> GetAllFormsFromDB()
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Form.NoTracking();
            return entitiesReadOnly.Form.ToList();
        }

        private Form GetFormFromDb(CompEntities entities, string formName)
        {
            return (from f in entities.Form
                    where f.Name.ToLower() == formName.ToLower()
                    select f).FirstOrDefault();
        }

        #endregion

        public List<Form> GetForms(SoeFieldSettingType type)
        {
            //Uses CompDbCache
            return (from f in CompDbCache.Instance.Forms
                    where f.Type == (int)type
                    select f).ToList();
        }

        public Form GetForm(string formName, SoeFieldSettingType type)
        {
            //Uses CompDbCache
            return (from f in CompDbCache.Instance.Forms
                    where f.Name.ToLower() == formName.ToLower() &&
                    f.Type == (int)type
                    select f).FirstOrDefault();
        }

        public Form GetMobileForm(TermGroup_MobileForms mobileForm)
        {
            string formName = ((int)mobileForm).ToString();
            return GetForm(formName, SoeFieldSettingType.Mobile);
        }

        /// <summary>
        /// Inserts a Form, if doesnt already exists
        /// </summary>
        /// <param name="formName">The Form name</param>
        /// <param name="type">The type</param>
        /// <returns>ActionResult</returns>
        public Form AddForm(string formName, SoeFieldSettingType type)
        {
            Form form = null;

            lock (formsLock)
            {
                CompDbCache.Instance.FlushForms();

                using (CompEntities entities = new CompEntities())
                {
                    #region Perform

                    form = GetFormFromDb(entities, formName);
                    if (form == null)
                    {
                        form = new Form()
                        {
                            Name = formName,
                            Type = (int)type,
                        };

                        if (!AddEntityItem(entities, form, "Form").Success)
                        {
                            form = null;
                            if (log.IsWarnEnabled) log.Warn("Could not add Form " + formName);
                        }
                    }

                    #endregion
                }
            }

            return form;

        }

        #endregion

        #region Field

        #region From DB

        public List<Field> GetFieldsFromDb()
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Field.NoTracking();
            return entitiesReadOnly.Field.Include("Form").ToList();
        }

        private Field GetFieldFromDb(CompEntities entities, string fieldName, int formId, SoeFieldSettingType type)
        {
            return (from f in entities.Field
                    where f.Name.ToLower() == fieldName.ToLower() &&
                    f.Form.FormId == formId &&
                    f.Type == (int)type &&
                    f.Form.Type == (int)type
                    select f).FirstOrDefault();
        }

        #endregion

        public List<Field> GetFields(int formId)
        {
            //Uses CompDbCache
            return (from f in CompDbCache.Instance.Fields
                    where f.Form.FormId == formId
                    select f).ToList();
        }

        public List<Field> GetFields(string formName, SoeFieldSettingType type)
        {
            //Uses CompDbCache
            return (from f in CompDbCache.Instance.Fields
                    where f.Form.Name.ToLower() == formName.ToLower() &&
                    f.Form.Type == (int)type &&
                    f.Type == (int)type
                    select f).ToList();
        }

        public Field GetField(string fieldName, string formName, SoeFieldSettingType type)
        {
            //Uses CompDbCache
            return (from f in CompDbCache.Instance.Fields
                    where f.Name.ToLower() == fieldName.ToLower() &&
                    f.Form.Name.ToLower() == formName.ToLower() &&
                    f.Type == (int)type &&
                    f.Form.Type == (int)type
                    select f).FirstOrDefault();
        }

        public Field GetField(string fieldName, int formId, SoeFieldSettingType type)
        {
            //Uses CompDbCache
            return (from f in CompDbCache.Instance.Fields
                    where f.Name.ToLower() == fieldName.ToLower() &&
                    f.Form.FormId == formId &&
                    f.Type == (int)type &&
                    f.Form.Type == (int)type
                    select f).FirstOrDefault();
        }

        public Field GetField(int fieldId, int formId)
        {
            //Uses CompDbCache
            return (from f in CompDbCache.Instance.Fields
                    where f.FieldId == fieldId &&
                    f.Form.FormId == formId
                    select f).FirstOrDefault();
        }

        /// <summary>
        /// Inserts a Field
        /// </summary>
        /// <param name="fieldName">The Field name</param>
        /// <param name="formName">The Form name</param>
        /// <param name="readOnly">True if the Field is set to ReadOnly</param>
        /// <param name="addForm">True if the Form should be inserted if it doesnt exist, otherwise false</param>
        /// <param name="type">The type</param>
        /// <returns>True if the Field was inserted, otherwise false</returns>
        public Field AddField(string fieldName, string formName, bool readOnly, SoeFieldSettingType type)
        {
            Field field = null;

            lock (fieldsLock)
            {
                CompDbCache.Instance.FlushFields();

                using (CompEntities entities = new CompEntities())
                {
                    #region Perform

                    //Get from db
                    Form form = GetFormFromDb(entities, formName);
                    if (form != null)
                    {
                        field = GetFieldFromDb(entities, fieldName, form.FormId, type);
                        if (field == null)
                        {
                            field = new Field()
                            {
                                Name = fieldName,
                                ReadOnly = readOnly,
                                Form = form,
                                Type = (int)type,
                            };

                            if (!AddEntityItem(entities, field, "Field").Success)
                            {
                                field = null;
                                if (log.IsWarnEnabled) log.Warn("Could not add Field " + fieldName);
                            }
                        }
                    }

                    #endregion
                }
            }

            return field;
        }

        #endregion

        #region SysSetting

        /// <summary>
        /// Get all SysSetting's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysSetting> GetSysSettings()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysSetting
                            .Include("SysSettingType")
                            .ToList<SysSetting>();
        }

        /// <summary>
        /// Get a SysSetting from SOESys database
        /// </summary>
        /// <param name="settingId">The SysSettingId</param>
        /// <returns>The SysSetting entity with the given name</returns>
        public SysSetting GetSysSetting(int settingId)
        {
            //Uses SysDbCache
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from ss in sysEntitiesReadOnly.SysSetting
                    where ss.SysSettingId == settingId &&
                    ss.SysSettingType.SysSettingTypeId == (int)SoeSettingType.Field
                    select ss).FirstOrDefault<SysSetting>();
        }

        #endregion

        #region SysSettingType

        /// <summary>
        /// Get all SysSettingType's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysSettingType> GetSysSettingTypes()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysSettingType
                            .ToList<SysSettingType>();
        }

        #endregion
    }
}
