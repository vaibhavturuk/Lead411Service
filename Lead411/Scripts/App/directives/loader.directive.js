angular.module('LoaderDirective', [])
  .directive('loader', function () {
      return {
          restrict: 'E',
          replace: true,
          template: '<div><div id="loader" class="modal fade in" id="overlayDiv" tabindex="-1" role="dialog" style="display: block;background: #A7a7a7; -ms-opacity: 0.2; opacity: 0.2"></div><div class="image"><img src="../../../Images/loader.png"></div></div>',
          link: function (rootScope, element, attr) {
              rootScope.$watch('loading', function (val) {
                  if (val) {
                      $(element).show();
                  } else {
                      $(element).hide();
                  }
              });
          }
      }
  });