<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
	<xsl:output method="xml" indent="yes"/>

	<xsl:template match="data">
		<table class="Grid" cellpadding="5" border="1" style="margin:0px auto;border-collapse:collapse;border:1px solid silver;">
			<th>
				<td bgcolor="#CCCCCC" style="letter-spacing:-1px;">
					<b>Entry</b>
				</td>
				<td bgcolor="#CCCCCC" style="letter-spacing:-1px;">
					<b>File Type</b>
				</td>
				<xsl:for-each select="columns/DataType" >
					<td bgcolor="#CCCCCC" style="letter-spacing:-1px;">
						<b>
							<xsl:value-of select="."/>
						</b>
					</td>
				</xsl:for-each>
			</th>
			<xsl:for-each select="files">
				<tr> 
					<td>
						<xsl:choose>
							<xsl:when test="FileType='Directory'">
								<img src="ComponentArtTreeView/images/folder.gif" />
							</xsl:when>
							<xsl:otherwise>
								<img src="ComponentArtTreeView/images/file.gif"/>
							</xsl:otherwise>
						</xsl:choose>
					</td>
					<td>
						<xsl:value-of select="Entry"/>
					</td>
					<td>
						<xsl:value-of select="FileType"/>
					</td>
					<xsl:for-each select="MetaData/field" >
						<td>
							<xsl:value-of select="value" />
							
						</td>
					</xsl:for-each>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:template>
</xsl:stylesheet>
