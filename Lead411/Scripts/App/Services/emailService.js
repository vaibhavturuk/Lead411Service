routerApp.factory('emailService', ['$http', function ($http) {

    return {

        // For Sending Mail
        SendEmail: function (email) {
            return $http.post('api/AdminApi/SendEmail', email);
        },

    };

}]);