message("Starting the script.\n")

message("Loading required packages...\n")

if(!require(dplyr)){
  install.packages("dplyr")
  library(dplyr)
}

if(!require(rjson)){
  install.packages("rjson")
  library(rjson)
}

if(!require(readr)){
  install.packages("readr")
  library(readr)
}

if(!require(RODBC)){
  install.packages("RODBC")
  library(RODBC)
}


#############
# Arguments #
#############

message("Getting required commandline parameters...\n")

args <- commandArgs(trailingOnly = TRUE)
message(args)
if(length(args) < 4) {
  #stop("Didn't get all the arguments needed.")
  arg_season <- "2018-19"
  arg_season_type <-"Regular+Season"
  arg_team_id <- "1610612744"
  arg_player_id <- "201142"
} else {
  arg_season <- args[1]
  arg_season_type <- args[2]
  arg_team_id <- args[3]
  arg_player_id <- args[4]
  working_dir <- args[5]
  setwd(working_dir)
}

message(paste("Working directory:",
              getwd()))

########
# Main #
########

message("Reading the local file...\n")

# First, read the local file to understand which game is already done processed
file_name_team_games <- paste0(paste(arg_season,
                                     arg_season_type,
                                     arg_team_id,
                                     sep = "_"),
                               ".csv")
games_done <- c()
if(file.exists(file_name_team_games)){
  message(paste(file_name_team_games, "exists.\n"))
  df <- read_csv(file_name_team_games,
                 col_types = list(col_character(), # Team_ID
                                  col_character(), # Game_ID
                                  col_character(), # GAME_DATE
                                  col_character(), # MATCHUP
                                  col_character(), # WL
                                  col_integer(),   # W
                                  col_integer(),   # L
                                  col_integer()    # Game_Index
                                  ),
                 locale = readr::locale(encoding = "UTF-8"))
  games_done <- df$Game_ID
} else {
  message(paste(file_name_team_games, "doesn't exist.\n"))
}

# Get list of games finished for the team
url_teamlog <- paste0("https://stats.nba.com/stats/teamgamelog",
                          "?Season=",
                          arg_season,
                          "&SeasonType=",
                          arg_season_type,
                          "&TeamID=",
                          arg_team_id)

message(paste("Accessing endpoint:", url_teamlog, "\n"))

res_teamlog <- rjson::fromJSON(file = url_teamlog)

message("JSON parse finished\n")

cols_games <- res_teamlog$resultSets[[1]]$headers
num_games <- length(res_teamlog$resultSets[[1]]$rowSet)
games_teamlog <- data.frame()
for (i in 1:num_games) {
  arrayRow <- as.character(res_teamlog$resultSets[[1]]$rowSet[[i]])
  df <- as.data.frame(matrix(arrayRow, nrow = 1),
                      stringsAsFactors = FALSE)
  colnames(df) <- cols_games
  games_teamlog <- rbind(games_teamlog, df)
}
games_teamlog$Game_ID <- as.character(games_teamlog$Game_ID)
games_teamlog %<>%
  arrange(Game_ID) %>%
  mutate(Game_Index = row_number())

# Check the new games came from the API compared to the local file
games_new <- subset(games_teamlog, !(Game_ID %in% games_done))
count_games_new <- nrow(games_new)
message(paste("# of new games found: ", count_games_new))


db_conn <- NULL

for(row in 1:count_games_new){
  
  # Connect to the DB if it hans't
  if(is.null(db_conn)){
    db_conn <- odbcDriverConnect("Driver=ODBC Driver 17 for SQL Server;Server=XXXXX.database.windows.net,1433; Database=;Uid=; Pwd=-N;")
  }
  
  new_team_id <- games_new[row, ]$Team_ID
  new_game_id <- games_new[row, ]$Game_ID
  new_game_date <- games_new[row, ]$GAME_DATE
  new_matchup <- games_new[row, ]$MATCHUP
  new_wl <- games_new[row, ]$WL
  new_w <- games_new[row, ]$W
  new_l <- games_new[row, ]$L
  new_game_index <- games_new[row, ]$Game_Index

  # Calling an API to get the boxscore of the new game
  url_boxscore <- paste0("https://stats.nba.com/stats/boxscoretraditionalv2",
                         "?EndPeriod=1&EndRange=0",
                         "&GameID=",
                         new_game_id,
                         "&RangeType=0&StartPeriod=1&StartRange=0")
  
  message(paste("Accessing endpoint:", url_teamlog, "\n"))
  
  res_boxscore <- rjson::fromJSON(file = url_boxscore)

  message("JSON parse finished\n")
  
  boxscores <- data.frame()
  
  cols_boxscore <- res_boxscore$resultSets[[1]]$headers
  for (i in 1:length(res_boxscore$resultSets[[1]]$rowSet)) {
    arrayRow <- as.character(res_boxscore$resultSets[[1]]$rowSet[[i]])
    df <- as.data.frame(matrix(arrayRow, nrow = 1),
                        stringsAsFactors = FALSE)
    colnames(df) <- cols_boxscore
    boxscores <- rbind(boxscores, df)
  }
  
  # Get the boxscore of THAT PLAYER specified
  boxscore_the_player <- subset(boxscores, PLAYER_ID == arg_player_id)
  
  # Connect to SQL Server
  initdata <- sqlQuery(db_conn ,paste("INSERT INTO NbaPlayerLog(TeamId, PlayerId, GameId)",
                                      "VALUES('",
                                      arg_team_id,
                                      "','",
                                      arg_player_id,
                                      "','",
                                      new_game_id,
                                      "'",
                                      ")"
                                      ))
  message('DB INSERT done.')
}

if(is.null(db_conn)){
  odbcClose(db_conn)
}

#############################
# Overriding the local file #
#############################

#message("Overriding teamlog data to the file to update the info...\n")
#write.csv(games_teamlog[, c("Team_ID", "Game_ID", "GAME_DATE", "MATCHUP", "WL", "W", "L", "Game_Index")],
#          file = file_name_team_games,
#          row.names = FALSE,
#          fileEncoding = "UTF-8")

message("Finishing the script.\n")
