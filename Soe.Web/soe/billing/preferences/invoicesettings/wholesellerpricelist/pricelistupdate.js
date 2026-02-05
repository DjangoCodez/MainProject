

function upgrade(id1,id2,id3) {
    var href="/modalforms/NotifyPriceListUpdate.aspx?sysPriceListHeadId="+id1+"&sysWholeSellerId="+id2+"&c="+id3;
    PopLink.modalWindowShow(href);
}
