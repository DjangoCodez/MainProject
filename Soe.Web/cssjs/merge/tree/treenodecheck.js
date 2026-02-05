function OnTreeNodeChecked(event) {
    var obj = event.srcElement;
    if(!obj)
        obj = event.target;
    var treeNodeFound = false;
    var checkedState;
    if (obj.tagName == "INPUT" && obj.type == "checkbox") 
    {
        var treeNode = obj;
        checkedState=treeNode.checked;
        do
        {
            obj = obj.parentNode;
        }
        while(obj.tagName!="TABLE")
        var parentTreeLevel = obj.rows[0].cells.length;
        var parentTreeNode = obj.rows[0].cells[0];
        var tables = obj.parentNode.getElementsByTagName("TABLE");
        var numTables = tables.length;
        var cbx = document.getElementById('AlternativeCheck');
        if (numTables >= 1) {
            for (var i=0; i < numTables; i++)
            {
                if (tables[i] == obj)
                {
                    treeNodeFound = true;
                    i++;
                    
                    if(checkedState)
                        checkRequiredParents(event.srcElement.parentElement);
                        
                    if (i == numTables)
                    {
                        return;
                    }
                }
                
                if(!checkedState)
                {
                    if (cbx != null) {
                        if (cbx.checked && (cbx.checked || cbx.checked == 'checked'))
                            return;
                    }              
                }
                if (treeNodeFound == true) {
                    if (cbx != null) {
                        if (cbx.checked && (cbx.checked || cbx.checked == 'checked'))
                            return;
                    }
                    var childTreeLevel=tables[i].rows[0].cells.length;                    
                    if (childTreeLevel > parentTreeLevel)
                    {
                        var cell=tables[i].rows[0].cells[childTreeLevel-1];
                        var inputs = cell.getElementsByTagName("INPUT");
                        if(inputs[0])
                            inputs[0].checked = checkedState;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
    }
}
function checkRequiredParents(td) {
    if(td==null) return;
    if(td.tagName=="TD") {
        var tr=td.parentElement;
        if(tr.tagName=="TR") {
            var tb=tr.parentElement;
            if(tb.tagName=="TBODY") {
                var table=tb.parentElement;
                if(table.tagName=="TABLE") {
                    recursiveCheck(table);
                }
            }
        }
    }
}
function recursiveCheck(current) {
    var parent=current.parentElement;
    var grandparent=current.parentElement.parentElement;
    if(parent.tagName=="DIV") {
        for(var i=0;i<grandparent.children.length;i++) {
            if(grandparent.children[i]==parent) { 
                var table = grandparent.children[i-1];
                if(table.tagName=="TABLE") {
                    var inputs=table.getElementsByTagName("INPUT");
                    if(inputs[0])
                        inputs[0].checked=true;
                    recursiveCheck(table);
                }
            }
        }
    }
}