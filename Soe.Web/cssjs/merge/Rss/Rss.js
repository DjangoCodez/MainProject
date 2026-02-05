var maxImgSrc = '/img/navigate_up.png';
var minImgSrc = '/img/navigate_down.png';

function showRssItem(id, show) {
    var header = document.getElementById("rssItemHeader" + id);
    var body = document.getElementById("rssItemBody" + id);
    if (header == null || body == null)
        return;

    var img = header.getElementsByTagName('img')[0];
    if(img == null)
        return;

    if (show == null)
        show = body.style.display == 'none';
        
    if(show) {
        body.style.display = '';
        img.src = minImgSrc;
    }
    else {
        body.style.display = 'none';
        img.src = maxImgSrc;
    }
}
