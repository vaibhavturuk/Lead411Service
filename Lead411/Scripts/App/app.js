var routerApp = angular.module('routerApp', ['ui.router', 'ui.bootstrap', 'LoaderDirective']);

routerApp.config(function($stateProvider, $urlRouterProvider) {
    
    $urlRouterProvider.otherwise('/user');
    
    $stateProvider
        
        // HOME STATES AND NESTED VIEWS ========================================

        .state('user', {
            url: '/user',
            templateUrl: 'Scripts/App/User/Views/userlist.html',
            controller: 'userController'
        })
        .state('home', {
            url: '/home',
            templateUrl: 'Scripts/App/Home/Views/partial-home.html'
        })
        // nested list with custom controller
        .state('home.list', {
            url: '/list',
            templateUrl: 'Scripts/App/Home/Views/partial-home-list.html',
            controller: function($scope) {
                $scope.dogs = ['Bernese', 'Husky', 'Goldendoodle'];
            }
        })
        // nested list with just some random string data
        .state('home.paragraph', {
            url: '/paragraph',
            template: ''
        })
        // ABOUT PAGE AND MULTIPLE NAMED VIEWS =================================
        .state('about', {
            url: '/about',
            views: {
                '': { templateUrl: 'Scripts/App/About/Views/partial-about.html' },
                'columnOne@about': { template: 'Look I am a column!' },
                'columnTwo@about': { 
                    templateUrl: 'Scripts/App/Home/Views/table-data.html',
                    controller: 'scotchController'
                }
            }
        });


        
});

