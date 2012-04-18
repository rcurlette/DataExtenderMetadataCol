Tridion-DataExtender-Add-Metadata-Column
------------------------
- Uses Tridion 2011 SP1.  Need to add references from Tridion/bin/client and also Tridion/web/WebUI/WebRoot/bin
- Set the metadata fieldname.  Open the AddMetadataColumn.cs file, change 'article_number' to your fieldname.
- Compile.  Copy ALL files in the VS output folder (/bin/Debug) to the CMS Server at /Tridion/web/WebUI/WebRoot/bin.
-  Create a new Folder on the CMS server for your DataExtender GUI Extension.  For example, create the DataExtender folder here: /Tridion/web/WebUI/Editors/DataExtender 
-  Copy the DataExtender.config file to the folder above in step 4.
-  Add the DataExtender config location to the System.config file in Tridion\web\WebUI\WebRoot\Configuration\System.config.

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
