Tridion-DataExtender-Add-Metadata-Column
------------------------
- Uses Tridion 2011 SP1.  
- References used:
     Tridion/bin/client/PIA
     - Tridion.ContentManager.Interop.cm_defines
     - Tridion.ContentManager.Interop.cm_tom
    
     Tridion/bin/client
     - Tridion.Common
    
     Tridion/web/WebUI/WebRoot/bin
     - Tridion.Web.UI.Core
     - Tridion.Web.UI.Models.TCM54
    
- Set the metadata fieldname.  Open the AddMetadataColumn.cs file, change 'article_number' to your fieldname.
- Compile.  Copy ALL files in the VS output folder (/bin/Debug) to the CMS Server at /Tridion/web/WebUI/WebRoot/bin.
- Create a new Folder on the CMS server for your DataExtender GUI Extension.  For example, create the DataExtender folder here: /Tridion/web/WebUI/Editors/DataExtender 
- Copy the DataExtender.config file to the folder above in step 4.
- Add the DataExtender config location to the System.config file in Tridion\web\WebUI\WebRoot\Configuration\System.config.
- DLLS used:

<editors default="CME">
  ...
  <editor name="DataExtender">
    <!-- DLL Files for DataExtender to be deployed to /Tridion/web/WebUI/WebRoot/bin -->
    <installpath>
     C:\Program Files (x86)\Tridion\web\WebUI\Editors\DataExtender\
    </installpath>
    <configuration>DataExtender.config</configuration>
    <vdir/>
  </editor>
</editors>

-  Refresh the GUI and behold your new GUI ListView Column
