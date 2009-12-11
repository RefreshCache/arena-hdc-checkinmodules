<%@ Control Language="C#" CodeFile="DisplayBoard.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.CheckIn.DisplayBoard" %>

<style>
body {
    background-color: black;
    cursor: url(UserControls/Custom/HDC/CheckIn/Include/transparent.gif), auto;
}

#mainHolder {
    width: 100%;
    position: absolute;
    top: 0px;
    left: 0px;
    right: 0px;
    bottom: 20%;
    text-align: center;
}

#bottomHolder {
    width: 100%;
    position: absolute;
    top: 80%;
    left: 0px;
    right: 0px;
    bottom: 0px;
    text-align: center;
}

#mainContent {
    font-family: "Arial Rounded MT Bold";
    color: #d0d0d0;
    position: relative;
    border: 0px 0px 0px 0px;
    margin: 0px 0px 0px 0px;
    padding: 0px 0px 0px 0px;
}

#bottomContent {
    font-family: "Arial Rounded MT Bold";
    color: #d0d0d0;
    position: relative;
    border: 0px 0px 0px 0px;
    margin: 0px 0px 0px 0px;
    padding: 0px 0px 0px 0px;
}

</style>

<script src="http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js"></script>
<script>

var lastID = -1;

function scaleToFit(obj)
{
  var i, targetWidth, targetHeight;

  targetWidth = obj.parent().attr("offsetWidth");
  targetHeight = obj.parent().attr("offsetHeight");

  for (i = 0; i < 2000; i++) {
    obj.css("font-size",i + "em");
    if (obj.attr("offsetWidth") > targetWidth || obj.attr("offsetHeight") > targetHeight) {
      obj.css("font-size", (i - 1) + "em");
      obj.css("top", ((targetHeight - obj.attr("offsetHeight")) / 2) + "px");
      break;
    }
  }
}

function loadData(data)
{
  var obj;

  obj = $("#topContent");
  obj.html("");
  scaleToFit(obj);

  obj = $("#mainContent");
  obj.html(data["Number"]);
  scaleToFit(obj);

  obj = $("#bottomContent");
  obj.html(data["Name"]);
  scaleToFit(obj);

  if (typeof(data["ID"]) == "undefined")
    lastID = -1;
  else {
    lastID = data["ID"];

    if (lastID != "-1" && typeof(window.Kiosk) != "undefined")
      window.Kiosk.UpdateSystemActivity();
  }
}

function loadNextPage()
{
  $.get("<%= Request.RawUrl %>", { format: "xml", lastID: lastID }, parseHtml, "html");
}

$(document).ready(function()
{
  loadNextPage();
  setInterval("loadNextPage();", 7500);
});

function parseHtml(xml)
{
  var data = new Object;
  var jData;

  data["ID"] = $(xml).find("ID").text();
  data["Number"] = "";
  data["Name"] = "";
  jData = $($(xml).find("Data").text());
  jData.each(function() {
    var item = $(this);
    if (item.attr("id") == "SecurityCode")
        data["Number"] = item.text();
    if (item.attr("id") == "Name")
        data["Name"] = item.text();
  });

  loadData(data);
}
</script>

<div id="topHolder"><span id="topContent"></span></div>
<div id="mainHolder"><span id="mainContent"></span></div>
<div id="bottomHolder"><span id="bottomContent"></span></div>
