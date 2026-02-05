import '../../Module';

import { CalendarService } from "../CalendarService";

angular.module("Soe.Manage.Calendar.SchoolHoliday.Module", ['Soe.Manage'])
    .service("calendarService", CalendarService);
