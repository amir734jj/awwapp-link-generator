angular.module('myApp', ['angular-loading-bar'])
    .controller('linkGenCtrl', ["$scope", "$http", function($scope, $http) {
    
    $scope.count = 1;
    $scope.prefix = "Group";
    $scope.links = [];
    $scope.text = "";
    
    $scope.getLinks = function (e) {
        e.preventDefault();
        
        $http.get("/" + $scope.count).then(function (response) {
            $scope.links = response.data;
            $scope.text = $scope.links.map(function (link, index) {
                return $scope.prefix + " " + (index + 1) + ":" + "\n" + link + "\n";
            }).join("\n") + "\n";
        });
    }
    
    $scope.copyText = function () {
        const el = document.createElement('textarea');
        el.value = $scope.text;
        document.body.appendChild(el);
        el.select();
        document.execCommand('copy');
        document.body.removeChild(el);
    }
}]);