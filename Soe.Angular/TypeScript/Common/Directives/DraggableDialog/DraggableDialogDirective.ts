export class DraggableDialogDirectiveFactory {
    //@ngInject
    public static create($document): ng.IDirective {
        return {
            restrict: 'EA',
            link: function (scope, element) {
                var header = $(element).find('.modal-header');
                if (!header)
                    return;

                var startX = 0;
                var startY = 0;

                header.on('mousedown', function (event) {
                    // Prevent default dragging of selected content
                    event.preventDefault();
                    startX = element.offset().left - event.pageX;
                    startY = element.offset().top - event.pageY;

                    $document.on('mousemove', mousemove);
                    $document.on('mouseup', mouseup);
                });

                function mousemove(event) {
                    element.offset({
                        top: event.pageY + startY,
                        left: event.pageX + startX
                    });
                }

                function mouseup() {
                    $document.unbind('mousemove', mousemove);
                    $document.unbind('mouseup', mouseup);
                }
            }
        };
    }
}