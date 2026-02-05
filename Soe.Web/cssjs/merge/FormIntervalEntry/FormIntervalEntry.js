function toggleInterval(tableId, imgId, intervalCounterId, noOfControls) 
{ 
    //find hidden counter field
    var counter = document.getElementById(intervalCounterId);
    if (counter == null)
        return;
    
    //changeimage
    var close = "/img/navigate_up.png";
    var open = "/img/navigate_down.png";
    var img = document.getElementById(imgId); 
    if (img != null) 
    { 
        var bImage = img.src.indexOf(open) >= 0; 
        if (!bImage)
        {
            img.src = open;
            counter.value = 1;
        }
        else 
        {
            img.src = close;
            counter.value = noOfControls;
        }
    }     
    
    //expand/collapse
    var table = document.getElementById(tableId); 
    if (table == null)
        return; 

    var bExpand = table.style.display == '';
    table.style.display = (bExpand ? 'none' : '');
}

function increaseInterval(tableId, intervalCounterId, noOfControls)
{
    //find hidden counter field
    var counter = document.getElementById(intervalCounterId);
    if (counter == null)
        return;

    //increase counter (max noOfControls)
    var value = parseInt(counter.value);
    var newValue = value + 1;
    if(newValue > noOfControls)
        return;
    
    //show
    var table = document.getElementById(tableId + "-" + newValue);  
    if (table == null)  
        return;

    table.style.display = '';
    
    //set counter
    counter.value = newValue;
}

function decreaseInterval(tableId, intervalCounterId, noOfControls)
{
    //find hidden counter field
    var counter = document.getElementById(intervalCounterId);
    if (counter == null)
        return;

    //decrease counter (minimum 1)
    var value = parseInt(counter.value);
    if (value == 1)
        return;
    
    //hide
    var table = document.getElementById(tableId + "-" + value); 
    if (table == null)  
        return; 
    table.style.display = 'none';
    
    //set counter
    counter.value = value - 1;
}

function deleteInterval(labelId, fromId, toId, tableId, intervalCounterId, intervalNo)
{
    try
    {
        //get mandatory objects
        var row = document.getElementById(tableId + "-" + intervalNo);
        var from = document.getElementById(fromId);
        var intervalCounter = document.getElementById(intervalCounterId);
        if (row == null || from == null || intervalCounter == null)
            return;

        //get optional objects
        var to = document.getElementById(toId); //dont exists if OnlyFrom property is true

        //get counter
        var counter = parseInt(intervalCounter.value);

        var firstRow = false;

        //check that counter dont go below 1
        if (counter == 1)
            firstRow = true;

        //check that first row dont gets deleted
        var suffix = '-1';
        if (labelId.length >= suffix.length) {
            var start = labelId.length - suffix.length;
            var stop = labelId.length;
            if (labelId.substring(start, stop) == suffix)
                firstRow = true;
        }

        if (firstRow == false)
        {
            //hide row
            row.style.display = 'none';

            //update counter
            intervalCounter.value = counter - 1;
        }

        // Fix on legacy code for setting empty value on dubble select
        // TODO: rewright JS function to handle more inputs.
        if (tableId == "AttestTransitions-FormIntervalEntry" && firstRow == true) {
            var attestTransitionsLabel = document.getElementById("AttestTransitions-label-1");
             attestTransitionsLabel.value = '';
        }

        if (tableId == "EDIToOrderTransferRules-FormIntervalEntry" && firstRow == true) {
            var eDIToOrderTransferRulesLabel = document.getElementById("EDIToOrderTransferRules-label-1");
            eDIToOrderTransferRulesLabel.value = '';
        }

               
        //set mandatory values
        from.value = '';
        
        //set optional values
        if (to != null)
            to.value = '';
    }
    catch (e) {
        alert(e);
    }
}

function setValueIfEmpty(id, value, focus)
{
    var el = document.getElementById(id);
    if (el == null)
        return;

    if (el.value == "" || el.value == "0")
    {
        el.value = value;

        if (focus && (focus == 'true' || focus == "1"))
        {
            if (el.style.visibility == "hidden" || el.style.display == "none" || el.disabled == true)
            {
                //do something because it can't accept focus()
            }
            else
                el.focus();
        }
    }
}

function setValue(fromId, toId)
{
    var from = document.getElementById(fromId); 
    if (from == null)  
        return; 

    var to = document.getElementById(toId); 
    if (to == null)  
        return; 
    
    to.value = from.value;
}

function copyValue(fromId, toId, focus)
{
    var from = document.getElementById(fromId); 
    if (from == null)  
        return; 

    var to = document.getElementById(toId); 
    if (to == null)  
        return; 

    if (to.value == "" || to.value == "0")
    {
        to.value = from.value;
        if (focus && (focus == 'true' || focus == "1"))
        {
            if (to.style.visibility == "hidden" || to.style.display == "none" || to.disabled == true)
            {
                //do something because it can't accept focus()
            }
            else
                to.focus();
        }
    }
}
