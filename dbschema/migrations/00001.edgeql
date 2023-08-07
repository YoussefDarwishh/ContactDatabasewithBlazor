CREATE MIGRATION m1jcg2rqq4ru7urq4lmzmr5qdgkdpoiaegkj2cyt4e4uo5awgqqczq
    ONTO initial
{
  CREATE TYPE default::Contact {
      CREATE REQUIRED PROPERTY date_of_birth: cal::local_datetime;
      CREATE REQUIRED PROPERTY description: std::str;
      CREATE REQUIRED PROPERTY email: std::str;
      CREATE REQUIRED PROPERTY first_name: std::str;
      CREATE REQUIRED PROPERTY last_name: std::str;
      CREATE REQUIRED PROPERTY marriage_status: std::bool;
      CREATE REQUIRED PROPERTY password: std::str;
      CREATE REQUIRED PROPERTY role: std::str;
      CREATE REQUIRED PROPERTY title: std::str;
      CREATE REQUIRED PROPERTY username: std::str;
  };
};
