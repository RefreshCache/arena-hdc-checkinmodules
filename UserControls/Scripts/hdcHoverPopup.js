var hideDelay = 500;
var currentID;
var hideTimer = null;
var ajax = null;
var hideFunction = function()
{
    if (hideTimer)
        clearTimeout(hideTimer);
    hideTimer = setTimeout(function()
    {
        currentPosition = { left: '0px', top: '0px' };
        container.css('display', 'none');
    }, hideDelay);
};

var currentPosition = { left: '0px', top: '0px' };

// One instance that's reused to show info for the current person
var container = $('<div id="personPopupContainer">'
    + '<table width="" border="0" cellspacing="0" cellpadding="0" align="center" class="personPopupPopup">'
    + '<tr>'
    + '   <td class="corner topLeft"></td>'
    + '   <td class="top"></td>'
    + '   <td class="corner topRight"></td>'
    + '</tr>'
    + '<tr>'
    + '   <td class="left">&nbsp;</td>'
    + '   <td style="background-color: #FFF;"><div id="personPopupContent"></div></td>'
    + '   <td class="right">&nbsp;</td>'
    + '</tr>'
    + '<tr>'
    + '   <td class="corner bottomLeft">&nbsp;</td>'
    + '   <td class="bottom">&nbsp;</td>'
    + '   <td class="corner bottomRight"></td>'
    + '</tr>'
    + '</table>'
    + '</div>');

$(document).ready(function () { $('body').append(container); });

function popupOverObject(objectID, html)
{
    var jObj = $("#" + objectID)
    
    $(jObj).data('hoverHTML', html);
    $(jObj).mouseover(popupOver);
}

function popupOver(eventObject)
{
    if (!$(this).data('hoverIntentAttached'))
    {
        $(this).data('hoverIntentAttached', true);
        $(this).hoverIntent
        (
            // hoverIntent mouseOver
            function()
            {
                if (hideTimer)
                    clearTimeout(hideTimer);

                var pos = $(this).offset();
                var width = $(this).width();
                var reposition = { left: (pos.left + width) + 'px', top: pos.top - 5 + 'px' };

                // If the same popup is already shown, then don't requery
                if (currentPosition.left == reposition.left &&
                    currentPosition.top == reposition.top)
                    return;

                container.css({
                    left: reposition.left,
                    top: reposition.top
                });

                currentPosition = reposition;

                $('#personPopupContent').html($(this).data('hoverHTML'));

                container.css('display', 'block');
            },
            // hoverIntent mouseOut
            hideFunction
        );
        // Fire mouseover so hoverIntent can start doing its magic
        $(this).trigger('mouseover');
    }
}

$(document).ready(function () {
    // Allow mouse over of details without hiding details
    $('#personPopupContainer').mouseover(function () {
        if (hideTimer)
            clearTimeout(hideTimer);
    });

    // Hide after mouseout
    $('#personPopupContainer').mouseout(hideFunction);
});
