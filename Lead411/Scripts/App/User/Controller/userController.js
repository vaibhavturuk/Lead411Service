routerApp.controller('userController', ['$scope', 'userService', 'emailService', '$timeout', '$rootScope', function ($scope, userService, emailService, $timeout, $rootScope) {

    $scope.pagingInfo = {
        pageNumber: 1,
        pageSize: 5,                     // No of data to be shown at a time in Grid
        sortBy: "firstName",            // Default sorting by FirstName
        reverse: false,                // Default sorting - orderBy Ascending
        search: "",
        totalItems: 0                 // Total No of records in Db
    };

    $scope.users = [];
    $scope.userDetail = {};
    $scope.searchButtonText = "Send";
    $scope.master = false;
    $scope.loader = false;
    $scope.email = { EmailId: "", Subject: "", Body: "" };   // Email object to send email
    $scope.selectedUsers = [];                              // Array containing selected Users IDs.
    $scope.disabled = true;
    

    $rootScope.loading = false;
    // Functiom call for searching
    $scope.search = function () {
        $scope.pagingInfo.pageNumber = 1;
        $scope.users = [];
        $scope.loadUsers();
    };

    // Function called when Sorting
    $scope.sort = function (sortBy) {
        $scope.master = false;
        if (sortBy === $scope.pagingInfo.sortBy) {
            $scope.pagingInfo.reverse = !$scope.pagingInfo.reverse;   // desc
        } else {
            $scope.pagingInfo.sortBy = sortBy;
            $scope.pagingInfo.reverse = false;                  // asc
        }
        $scope.pagingInfo.pageNumber = 1;
        $scope.loadUsers();
    };

    // Function called when page changes
    $scope.selectPage = function (pageNumber) {
        $scope.loader = { loading: true };
        $scope.pagingInfo.pageNumber = pageNumber;
        $scope.selectedUsers = [];

        $scope.master = "";
        $scope.loadUsers();
    };

    // function to fetch list of data
    $scope.loadUsers = function () {
        $rootScope.loading = true;
        userService.GetUserList($scope.pagingInfo).then(function(response) {
            $scope.users = response.data.content.registeredUsers; // data to be displayed on current page.
            $scope.pagingInfo.totalItems = response.data.content.noOfRecords; // total data count.
            $rootScope.loading = false;
        });

    }
    
    // Initial table load
    $scope.loadUsers();

    // For Getting Individual User Details in Modal PopUp
    $scope.GetUserDetails = function (userMembershipId) {

        $scope.loader = true;

        // Unchecking checked checkboxes if checked
        angular.forEach($scope.users, function (item) {
            item.selected = false;
        });
        $scope.selectedUsers.length = 0;
        $scope.master = false;


        // Calling userService to get Individual user data
        userService.GetUserDetails(userMembershipId).then(function (result)
        {
            $scope.userDetail = result.data.content;
            $scope.disabled = false;
            $scope.loader = false;
        });
    }


    // --------------------------------------------------------- CheckBox functionality Starts

    // For checking all checkbox when master checkbox is checked
    $scope.isSelectAll = function () {

        $scope.selectedUsers = [];
       
        if ($scope.master) {
            $scope.master = true;
            for (var i = 0; i < $scope.users.length; i++) {
                $scope.selectedUsers.push({ 'UserId': $scope.users[i].userId, 'EmailId': $scope.users[i].emailId });
            }
            $scope.disabled = false;
        }
        else {
            $scope.master = false;
            $scope.disabled = true;
        }

        angular.forEach($scope.users, function (item) {
            item.selected = $scope.master;                   // Setting to false
        });

    }

    // For checking single checkbox
    $scope.isChecked = function (data) {
        if (this.user.selected) {
            $scope.selectedUsers.push({ 'UserId': this.user.userId, 'EmailId': this.user.emailId });
            if ($scope.selectedUsers.length === $scope.users.length) {
                $scope.master = true;
            }
            $scope.disabled = false;
        }
        else {
            $scope.master = false;
                for (var i = 0; i < $scope.selectedUsers.length; i++) {
                    if ($scope.selectedUsers[i].UserId == data.userId)
                    {
                        $scope.selectedUsers.splice(i, 1);
                        break;
                    }
                }
               
            if ($scope.selectedUsers.length === 0) {
                $scope.disabled = true;
            }
        }
    }

    // Function to call on row click
    $scope.rowClicked = function (data) {
        
        var found = false;

        for (var i = 0; i < $scope.selectedUsers.length; i++) {
                if ($scope.selectedUsers[i].UserId == data.userId) {
                    found = true;
                    $scope.selectedUsers.splice(i, 1);
                    $scope.master = false;
                    data.selected = false;
                    if ($scope.selectedUsers.length == 0) {
                        $scope.disabled = true;
                    }
                    break;
                }
            }

            if (!found) {
                $scope.selectedUsers.push({ 'UserId': data.userId, 'EmailId': data.emailId });
                data.selected = true;
                if ($scope.selectedUsers.length === $scope.users.length) {
                    $scope.master = true;
                }
                $scope.disabled = false;
            }
          
    }
        
    // --------------------------------------------------------- CheckBox functionality Ends



    // --------------------------------------------------------- PopUp For Action Buttons Starts
   
    // For Email PopUp
    $scope.emailPopUp = function () {
        $scope.email = { EmailId: "", Subject: "", Body: "" };

        $("#actionModal").modal("show");

        if ($scope.selectedUsers.length != 0)
        {
            angular.forEach($scope.selectedUsers, function (item) {
                $scope.email.EmailId += item.EmailId + ",";                   
            });

            var str = $scope.email.EmailId.slice(0, $scope.email.EmailId.length - 1);  // Trimming to remove last "," from email string
            $scope.email.EmailId = str;
        }
        else
        {
            $scope.email.EmailId = $scope.userDetail.emailId;
        }

    }
   
    // Function for sending Mail
    $scope.SendEmail = function (email)
    {
        $scope.searchButtonText = "Sending";
        emailService.SendEmail(email).then(function (response) {
            $timeout(function ()
            {
                $scope.searchButtonText = "Send";
                if (response.status === "200") {
                    $scope.email = { EmailId: "", Subject: "", Body: "" };
                    $("#actionModal").modal("hide");
                }
            }, 3000);
        });
    }

    // Closing Email modal and clearing data, If there
    $scope.closeEmailDailoug = function () {
        $scope.email = { EmailId: "", Subject: "", Body: "" };
        $('#actionModal').modal('hide');
    }

    // Closing User Detail Modal
    $scope.dismissDetailModal = function () {
        $('#myModal').modal('hide');
        $scope.disabled = true;
    }

    // --------------------------------------------------------- PopUp For Action Buttons Ends

}]);