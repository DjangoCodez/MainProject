# Migration guide - Short version

## Soe.Web  

* Add AngularSpaHost to default.aspx page.  

Full example:  
```
<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.preferences.schedulesettings.skilltype._default" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">  
    <%if (UseAngularSpa) {%>
       <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <%} else {%>
       <SOE:AngularHost ID="AngularHost" ModuleName="Time" AppName="Soe.Time.Schedule.SkillTypes" runat="server" />
        <script type="text/javascript">
            if (!soeConfig)
                soeConfig = {};
        </script>   
    <%}%> 
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
```

* Turn on the new page by changing page status for Test/REF to "AngularJS first".  
* Switch to the Angular page by clicking the Angular icon.

## Soe.Common  

* Make sure a DTO and a GridDTO exists and have the [TSInclude] attribute.  
* Make sure ToDTO(), ToDTOs(), ToGridDTO() and ToGridDTOs() extension methods exist.  

## Soe.Business  

### GetGrid method
* Make sure the method doesn't include unneccessary relations without any conditions. If so, add parameters to opt in for relations not needed for the grid.  
* Add support for passing in id in the method returning grid data (return only one, still as a list)  

Example:  
```
public List<SkillType> GetSkillTypes(int actorCompanyId, int? skillTypeId = null)
{
    IQueryable<SkillType> query = (from s in CompEntitiesReadOnly.SkillType
                                 where s.ActorCompanyId == actorCompanyId &&
                                 s.State == (int)SoeEntityState.Active
                                 select s);

    if (skillTypeId.HasValue)
        query = query.Where(s => s.SkillTypeId == skillTypeId.Value);

    List<SkillType> skillTypes = query.ToList();
    
    return skillTypes.OrderBy(s => s.Name).ToList();
}
```  

## Soe.Web.Api  

* Create (or use existing) controller in V2 directory  
* Copy relevant api calls  
  * GetGrid  
  * Get  
  * Save  
  * Delete  

As a coding standard these should be named like this:
* [HttpGet]  
  [Route("Grid/{skillTypeId:int?}")]  
  GetSkillTypesGrid(int? skillTypeId = null)  
* [HttpGet]  
  [Route("{skillTypeId:int}")]  
  GetSkillType(int skillTypeId)  
* [HttpPost]  
  [Route("")]  
  SaveSkillType(SkillTypeDTO model)  
* [HttpDelete]  
  [Route("{skillTypeId:int}")]  
  DeleteSkillType(int skillTypeId)  


* Add id to grid query  
* Also rename endpoint (route) to just "Grid". It probably had a name like GetSkillTypes before.  
  That makes all these four CRUD routes having the same name in Angular, regardless of entity.  

```  
[HttpGet]
[Route("Grid/{skillTypeId:int?}")]
public IHttpActionResult GetSkillTypesGrid(int? skillTypeId = null)
{
    return Content(HttpStatusCode.OK, cm.GetSkillTypes(base.ActorCompanyId, false, skillTypeId).ToGridDTOs());
}
```  

* Build the solution  

## Soe.Angular.Spa

* Use Angular CLI in the terminal window to create some new files.  

### 1. Modules
* In the terminal window, move to the top module (cd src/app/features/<TOP_MODULE>)  
* `ng g m <COMP_NAME> --routing` 

### 2. Container component
* Move to the newly created folder in the terminal (cd <COMP_NAME>)  
* `ng g c -t -s --no-standalone components/<COMP_NAME>`  

### 3. Grid component

* `ng g c -t -s --no-standalone components/<COMP_NAME>-grid`  

### 4. Edit component

* `ng g c -s --no-standalone components/<COMP_NAME>-edit`  

### 5. Add routing
* Open the **<COMP_NAME>-routing.module.ts** file.  
* In the routes property, add a reference to the new component:  
```
const routes: Routes = [
  {
    path: 'default.aspx',
    component: SkillTypesComponent,
  },
];
```

* Open the **<TOP_MODULE>-routing-module.ts** file.  
* In the routes property, add a reference to the new component:  
```
{
  path: 'preferences/schedulesettings/skilltype',
  loadChildren: () =>
    import('./skill-types/skill-types.module').then(m => m.SkillTypesModule),
},
```

**Add the routes in alphabetic order!**  

### 6. Service

* `ng g s services/<COMP_NAME>`  

### 7. Model

* `ng g cl models/<COMP_NAME>-form --type=model`  
* Start file watcher `npm run watch`  
* If there are no build errors, you can now go back to the browser and refresh the page.  

### Add content to your files  

#### 1. Service

* Open the <COMP_NAME>.service.ts file.  
* Add our own http client in the constructor:  
  ```
  constructor(private http: SoeHttpClient) {}
  ```
* Add methods for the four CRUD operations you created in the Web.Api controller.    

As an example this is how the get operation mentioned above is implemented in the service:
```
get(id: number): Observable<ISkillTypeDTO> {
    return this.http.get<ISkillTypeDTO>(getSkillType(id));
}
```

* The rest of the service methods should follow the same principle, and note that it's important that the names of the methods here is **getGrid()**, **get()**, **save()** and **delete()**.  

#### 2. Container component (tabs)  
* Open the **<COMP_NAME>.component.ts** file and remove the template and styles properties.  
* Add a templateUrl property that points to our generic template file:  
```
templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
```

* Remove the selector property.  
* Copy content from similar component and modify accordingly.  

#### 3. Grid component  
* Open the **<COMP_NAME>-grid.component.ts** file and remove the template and styles properties.  
* Add a templateUrl property that points to our generic grid template file:  
```
templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
```

* Add a providers property: `providers: [FlowHandlerService],`  
* Extend the grid base class and also implement OnInit:  
```
export class SkillTypesGridComponent
  extends GridBaseDirective<ISkillTypeGridDTO>
  implements OnInit {}
```

* Add the ngOnInit() method:
```
ngOnInit() {
  super.ngOnInit();

  this.startFlow(
    Feature.Time_Preferences_ScheduleSettings_SkillType,
    'Time.Schedule.SkillTypes'
  );
}
```

* Add call to *super.ngOnInit()*.  
* Specify the correct permission.  
* Specify grid name.  
* Override **gridReadyToDefine()** and add columns.  

#### 4. Create form model in <COMP_NAME>-form.model.ts  
* Look at similar page and copy relevant information.  

#### 5. Edit component
* Open <COMP_NAME>-edit.component.ts  
* Remove empty styles property.
* Add a providers property: `providers: [FlowHandlerService],`  
* Extend the edit base class and also implement OnInit:  
```
export class SkillTypesEditComponent
  extends EditBaseDirective<ISkillTypeDTO, SkillTypesService, SkillTypesForm>
  implements OnInit
```
* Inject the service: `service = inject(SkillTypesService);`  
* Add the ngOnInit() method:
```
ngOnInit() {
  super.ngOnInit();

  this.startFlow(Feature.Time_Preferences_ScheduleSettings_SkillType_Edit);
}
```

* Add call to *super.ngOnInit()*.  
* Specify the correct permission.  

**Now it's time for the view in the html file.**

* Open <COMP_NAME>-edit.component.html  
* Remove any auto generated content.  
* Add content...
