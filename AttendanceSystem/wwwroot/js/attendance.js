document.addEventListener('DOMContentLoaded', function() {
    const locationRequired = document.getElementById('locationRequired').value === 'true';
    const markAttendanceBtn = document.getElementById('markAttendanceBtn');
    const latitudeInput = document.getElementById('latitude');
    const longitudeInput = document.getElementById('longitude');
    const locationStatus = document.getElementById('locationStatus');
    
    if (locationRequired && markAttendanceBtn) {
        markAttendanceBtn.addEventListener('click', function(e) {
            e.preventDefault();
            
            if (navigator.geolocation) {
                locationStatus.textContent = 'Getting location...';
                
                navigator.geolocation.getCurrentPosition(
                    function(position) {
                        latitudeInput.value = position.coords.latitude;
                        longitudeInput.value = position.coords.longitude;
                        locationStatus.textContent = 'Location acquired!';
                        
                        // Submit the form
                        document.getElementById('attendanceForm').submit();
                    },
                    function(error) {
                        let errorMessage = 'Error getting location: ';
                        switch(error.code) {
                            case error.PERMISSION_DENIED:
                                errorMessage += 'User denied the request for Geolocation.';
                                break;
                            case error.POSITION_UNAVAILABLE:
                                errorMessage += 'Location information is unavailable.';
                                break;
                            case error.TIMEOUT:
                                errorMessage += 'The request to get user location timed out.';
                                break;
                            case error.UNKNOWN_ERROR:
                                errorMessage += 'An unknown error occurred.';
                                break;
                        }
                        locationStatus.textContent = errorMessage;
                    },
                    { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
                );
            } else {
                locationStatus.textContent = 'Geolocation is not supported by this browser.';
            }
        });
    }
});