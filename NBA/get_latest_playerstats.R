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

message(paste("Working directory:",
              getwd()))

#############
# Arguments #
#############

message("Getting required commandline parameters...\n")

args <- commandArgs(trailingOnly = TRUE)
message(args)
if(length(args) < 4) {
  #stop("Didn't get all the arguments needed.")
  season <- "2018-19"
  season_type <-"Regular+Season"
  team_id <- "1610612744"
  player_id <- "12345"
} else {
  season <- args[1]
  season_type <- args[2]
  team_id <- args[3]
  player_id <- args[4]
}

########
# Main #
########

message("Reading the local file...\n")

# First, read the local file to understand which game is already done processed
file_name_team_games <- paste0(paste(season,
                                     season_type,
                                     team_id,
                                     sep = "_"),
                               ".csv")
games_done <- c()
if(file.exists(file_name_team_games)){
  message(paste(file_name_team_games, "exists.\n"))
  df <- read.csv(file_name_team_games)
  games_done <- df$Game_ID
} else {
  message(paste(file_name_team_games, "doesn't exist.\n"))
}

# Get list of games finished for the team
url <- paste0("https://stats.nba.com/stats/teamgamelog",
              "?Season=",
              season,
              "&SeasonType=",
              season_type,
              "&TeamID=",
              team_id)

message(paste("Accessing endpoint:", url, "\n"))

res <- rjson::fromJSON(file = url)

message("JSON parse finished\n")

names_col <- res$resultSets[[1]]$headers
num_games <- length(res$resultSets[[1]]$rowSet)
games_teamlog <- data.frame()
for (i in 1:num_games) {
  arrayRow <- as.character(res$resultSets[[1]]$rowSet[[i]])
  df <- as.data.frame(matrix(arrayRow, nrow = 1),
                      stringsAsFactors = FALSE)
  colnames(df) <- names_col
  games_teamlog <- rbind(games_teamlog, df)
}
games_teamlog$Game_ID <- as.character(games_teamlog$Game_ID)
games_teamlog %<>%
  arrange(Game_ID) %>%
  mutate(Game_Index = row_number())

# Check the new games came from the API compared to the local file
games_new <- subset(games_teamlog, !(Game_ID %in% games_done))
message(games_new$Game_ID)

#############################
# Overriding the local file #
#############################

message("Overriding teamlog data to the file to update the info...\n")
write.csv(games_teamlog, file = file_name_team_games, row.names = FALSE)

message("Finishing the script.\n")
