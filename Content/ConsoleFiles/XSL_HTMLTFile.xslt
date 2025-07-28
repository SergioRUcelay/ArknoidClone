<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" encoding="UTF-8" indent="yes"/>

	<xsl:template match="/">
		<html>
			<head>
				<title>Command Logs</title>
			</head>
			<body>
				<xsl:apply-templates select="//CommandReply"/>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="CommandReply">
		<div style="margin-bottom: 1em; font-family: monospace;">

			<!-- Type -->
			<xsl:choose>
				<xsl:when test="Type = 'Error'">
					<div style="color: red;">
						<strong>
							<xsl:value-of select="Type"/>
						</strong>
					</div>
				</xsl:when>
				<xsl:when test="Type = 'Success'">
					<div style="color: green;">
						<strong>
							<xsl:value-of select="Type"/>
						</strong>
					</div>
				</xsl:when>
				<xsl:when test="Type = 'Void'">
					<div style="color: goldenrod;">
						<strong>
							<xsl:value-of select="Type"/>
						</strong>
					</div>
				</xsl:when>
			</xsl:choose>

			<!-- Return for list -->
			<xsl:if test="Return/Entries/FunctionEntry">
			<div>
				<strong>Available Functions:</strong>
				<table class="functions-table">
					<thead>
						<tr>
							<th>Function</th>
							<th>Description</th>
						</tr>
					</thead>
					<tbody>
						<xsl:for-each select="Return/Entries/FunctionEntry">
							<tr>
								<td>
									<strong>
										<xsl:value-of select="Function"/>
									</strong>
								</td>
								<td>
									<xsl:value-of select="Description"/>
								</td>
							</tr>
						</xsl:for-each>
					</tbody>
				</table>
				<div style="margin-top: 10px; font-size: 0.9em; color: #666;">
					Total functions: <xsl:value-of select="count(Return/Entries/FunctionEntry)"/>
				</div>
			</div>
			</xsl:if>
			<xsl:if test="Return and not(Return/Entries/FunctionEntry)">
				<xsl:choose>
					
				<!-- Return contains image -->
				<xsl:when test="contains(Return, 'iVBOR')">
				  <div>
					<img class="capture">
					  <xsl:attribute name="src">
						<xsl:text>data:image/png;base64,</xsl:text>
						<xsl:value-of select="Return"/>
					  </xsl:attribute>
						<xsl:attribute name="style">width:75%; height:auto;</xsl:attribute>
					</img>
				  </div>
				</xsl:when>

				<!-- Return not contains image -->
				<xsl:otherwise>
					<div style="color: blue;">
						<strong>Return:</strong>
						<xsl:value-of select="Return"/>
					</div>
				</xsl:otherwise>
			 </xsl:choose>
			</xsl:if>
			
			<!-- Timestamp -->
			<xsl:if test="Timestamp">
				<div>
					<strong>Timestamp:</strong>
				<xsl:value-of select="concat(substring(Timestamp, 6, 2), '/', substring(Timestamp, 9, 2), '/', substring(Timestamp, 1, 4), ' ',
					substring(Timestamp, 12, 2), 'h:', substring(Timestamp, 15, 2), 'm:',substring(Timestamp, 18, 2),'s')"/>
				</div>
			</xsl:if>

			<!-- Function Called -->
			<xsl:if test="FunctionCalled">
				<div>
					<strong>Function:</strong>
					<xsl:value-of select="FunctionCalled"/>
				</div>
			</xsl:if>

			<!-- Message -->
			<xsl:if test="Message">
				<div>
					<strong>Message:</strong>
					<xsl:value-of select="Message"/>
				</div>
			</xsl:if>

			<!-- ThrowError -->
			<xsl:if test="ThrowError">
				<div>
					<strong>ThrowError:</strong>
					<xsl:value-of select="ThrowError"/>
				</div>
			</xsl:if>
		</div>
	
	</xsl:template>
	<xsl:template match="result">
	  <xsl:choose>
		<xsl:when test="return/entries/functionentry">
		  <xsl:apply-templates select="return/entries/functionentry"/>
		</xsl:when>
		<xsl:otherwise>
		  <!-- Other processing -->
		</xsl:otherwise>
	  </xsl:choose>
	</xsl:template>

</xsl:stylesheet>