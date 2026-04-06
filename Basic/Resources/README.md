Dateitypen:

# TextureAtlas
- XML -> atxml
- JSON -> atjsn
- Binary -> atbin

# Resource
- XML -> rxml
- JSON -> rjsn
- Binary -> rbin


Aufbau einer Resource-Eintrag beim XML

Beispiel Resource mit IResourceEntry:

	<Resource Type="TextureAtlasSet" Id="AtlasLight">
		<Values VariantName="Light"/>
		<Resource Id="Cousine-Regular" Type="TextureAtlas" File="example_skin_1_light.axml">
			<Values ScaleDefinition="1"/>
		</Resource>
		<Resource Id="Cousine-Regular" Type="TextureAtlas" File="example_skin_2_light.axml">
			<Values ScaleDefinition="2"/>
		</Resource>
	</Resource>	
	
	<Resource Type="Font" Id="default-font">
		<Values Typeface="Noto" Size="20"/>
	</Resource>

	<Resource Id="ColorExample" Type="Color" Content="#ffffff"/>


Oder beim JSON

{
	"@type":"resource",
	"target-type":"Color",
	"content":"Yellow",
	"values":{ ... },
}