module default {
    type Contact {
        required first_name: str;
        required last_name: str;
        required email: str;
        required title: str;
        required description: str;
        required date_of_birth: cal::local_datetime;
        required marriage_status: bool;
        required username: str;
        required password: str;
        required role: str;
    }
}