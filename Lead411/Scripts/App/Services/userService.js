routerApp.factory('userService', ['$http', function ($http) {
   
   return {

       // For getting Users List with pagination, searching, sorting
       GetUserList: function (pagingInfo) {
           return $http.get('api/AdminApi/GetUserList?pageNumber=' + pagingInfo.pageNumber + '&PageSize=' + pagingInfo.pageSize + '&sortBy=' + pagingInfo.sortBy + '&reverse=' + pagingInfo.reverse + '&search=' + pagingInfo.search);
        },

        GetUserDetails : function (userMembershipId) {
            return $http.get('api/AdminApi/GetUserDetails?userMembershipId=' + userMembershipId);
        }

    };

}]);