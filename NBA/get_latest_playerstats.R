if(!require(dplyr)){
  install.packages("dplyr")
  library(dplyr)
}

if(!require(rjson)){
  install.packages("rjson")
  library(rjson)
}

#############
# Arguments #
#############

args <- commandArgs(trailingOnly = TRUE)
message(args)
if(length(args) < 4) {
  #stop("Didn't get all the arguments needed.")
  season <- "2018-19"
  seasonType <-"Regular+Season"
  teamId <- "1610612744"
  playerId <- "12345"
} else {
  season <- args[1]
  seasonType <- args[2]
  teamId <- args[3]
  playerId <- args[4]
}

#############
# API calls #
#############

# TeamGameLog
url <- paste0("https://stats.nba.com/stats/teamgamelog",
              "?Season=",
              season,
              "&SeasonType=",
              seasonType,
              "&TeamID=",
              teamId)

res <- rjson::fromJSON(file = url)

colNames <- res$resultSets[[1]]$headers
numGames <- length(res$resultSets[[1]]$rowSet)
dfTeamGames <- data.frame()
for (i in 1:numGames) {
  arrayRow <- as.character(res$resultSets[[1]]$rowSet[[i]])
  df <- as.data.frame(matrix(arrayRow, nrow = 1),
                      stringsAsFactors = FALSE)
  colnames(df) <- colNames
  dfTeamGames <- rbind(dfTeamGames, df)
}
dfTeamGames$Game_ID <- as.character(dfTeamGames$Game_ID)
dfTeamGames %<>%
  arrange(Game_ID) %>%
  mutate(Game_Index = row_number())

file_name_team_games <- paste0(paste(season, seasonType, teamId, sep = "_"), ".csv")
write.csv(dfTeamGames, file = file_name_team_games, row.names = FALSE)
