//
// Initialize the service after a postback event.
//
$(document).ready(function () {
    var j = JSON.parse($('#' + jsonDataField).val().replace(/&quote;/g, '"'));
    $('#' + jsonDataField).val(JSON.stringify(j));
    for (k in j) {
        var r = j[k];
        var room = CreateActiveRoom(k.replace('active', ''), r.title);
        $("#service ol:first").append(room);

        registerActiveRoom(room, true);

        for (i = 0; i < r.ats.length; i++) {
            var o = CreateActiveAttendanceType($(room).attr('id') + '_' + r.ats[i].id, r.ats[i].title, r.ats[i].age);
            $(room).find('ol').append(o);

            registerActiveAttendanceType(o, true);
        }
    }
});

//
// Create new active room.
//
function CreateActiveRoom(id, title) {
    var o = $('<li id="active' + id + '" class="sbActiveRoom"></li>');
    var header = $('<h3 class="ui-widget-header sbHeader"></h3>').text(title).appendTo(o);
    $('<span class="sbEraseButton ui-icon ui-icon-close"></span>').click(function () { removeActiveRoom($(this).parent().parent()); }).appendTo(header);
    var content = $('<div class="ui-widget-content"><ol></ol></div>').appendTo(o);

    return o;
}

//
// Create a new active attendance type.
//
function CreateActiveAttendanceType(id, title, age) {
    var o = $('<li id="' + id + '" class="ui-state-default sbListItem"></li>');
    o.data('age', age);
    o.append('<span>' + title + '</span>');
    $('<span class="sbEraseButton ui-icon ui-icon-close"></span>').click(function () { removeActiveAttendanceType($(this).parent()); }).appendTo(o);

    return o;
}

//
// Register for droppings.
//
$(function () {
    $(".sbAttendanceType").draggable({ appendTo: 'body', helper: 'clone' });
    $(".sbRoom").draggable({ appendTo: 'body', helper: 'clone' });

    $("#service ol").droppable({
        accept: '.sbRoom',
        activeClass: 'ui-state-default',
        hoverClass: 'ui-state-hover',
        drop: function (event, ui) {
            var o = CreateActiveRoom(ui.draggable.attr('id'), ui.draggable.text());
            o.appendTo(this);

            registerActiveRoom(o, false);
        }
    });
});

//
// Register a new active room. Setup the droppable zone and add the room
// id to our data.
//
function registerActiveRoom(room, isinit) {
    if (isinit == false) {
        //
        // Store in our data.
        //
        var j = JSON.parse($('#' + jsonDataField).val());
        if (room.attr('id') in j) {
            $(room).remove();
            return;
        }
        j[room.attr('id')] = { title: $(room).find('h3').text(), ats: [] };
        $('#' + jsonDataField).val(JSON.stringify(j));
    }

    //
    // Best guess sorting by name.
    //
    var mylist = $(room).parent();
    var listitems = mylist.children('li').get();
    listitems.sort(function (a, b) {
        var compA = $(a).text().toUpperCase();
        var compB = $(b).text().toUpperCase();
        return (compA < compB) ? -1 : (compA > compB) ? 1 : 0;
    });
    $.each(listitems, function (idx, itm) { mylist.append(itm); });

    //
    // Setup the droppable zone.
    //
    $(room).find("ol").droppable({
        accept: '.sbAttendanceType',
        activeClass: 'ui-state-default',
        hoverClass: 'ui-state-hover',
        drop: function (event, ui) {
            var o = CreateActiveAttendanceType(room.attr('id') + '_' + ui.draggable.attr('id'), ui.draggable.text(), ui.draggable.attr('data-age'));
            o.appendTo(this);

            registerActiveAttendanceType(o, false);
        }
    });

}

//
// Remove an active room from the list as well as our data.
//
function removeActiveRoom(room) {
    //
    // Remove from our data.
    //
    var j = JSON.parse($('#' + jsonDataField).val());
    delete j[room.attr('id')];
    $('#' + jsonDataField).val(JSON.stringify(j));

    //
    // Remove from DOM.
    //
    $(room).remove();
}

//
// Register a new active attendance type for a room by adding it to
// our data.
//
function registerActiveAttendanceType(atype, isinit) {
    if (isinit == false) {
        //
        // Store it in our data array.
        //
        var ids = $(atype).attr('id').split('_', 2);
        var j = JSON.parse($('#' + jsonDataField).val());
        var r = j[ids[0]];
        for (i = 0; i < r.ats.length; i++) {
            if (r.ats[i].id == ids[1]) {
                $(atype).remove();
                return;
            }
        }
        r.ats.push({ title: $(atype).find('span').text(), id: ids[1], age: $(atype).data('age') });
        $('#' + jsonDataField).val(JSON.stringify(j));
    }

    //
    // Best guess sorting by age.
    //
    var mylist = $(atype).parent();
    var listitems = mylist.children('li').get();
    listitems.sort(function (a, b) {
        var compA = ($(a).data('age') ? $(a).data('age') : $(a).text().toUpperCase());
        var compB = ($(b).data('age') ? $(b).data('age') : $(b).text().toUpperCase());
        return (compA < compB) ? -1 : (compA > compB) ? 1 : 0;
    });
    $.each(listitems, function (idx, itm) { mylist.append(itm); });
}

//
// Remove an active attendance type from the list and our data.
//
function removeActiveAttendanceType(atype) {
    //
    // Remove it from our data.
    //
    var ids = $(atype).attr('id').split('_', 2);
    var j = JSON.parse($('#' + jsonDataField).val());
    var r = j[ids[0]];
    for (i = 0; i < r.ats.length; i++) {
        if (r.ats[i].id == ids[1]) {
            r.ats.splice(i, 1);
            break;
        }
    }
    $('#' + jsonDataField).val(JSON.stringify(j));

    //
    // Remove it from the DOM.
    //
    $(atype).remove();
}
