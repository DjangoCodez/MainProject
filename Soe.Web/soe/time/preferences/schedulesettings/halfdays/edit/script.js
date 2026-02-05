
function ToggleInputFields(sender) {
    if(document.getElementById("Type")[clockValue1].selected) {
//    ||document.getElementById("Type")[clockValue2].selected||document.getElementById("Type")[clockValue3].selected
        if(sender==null) {
            sender=document.getElementById("Value");
            sender.value=sender.value.replace(',00','');
        }
        formatTime(sender)
    }   
 }

