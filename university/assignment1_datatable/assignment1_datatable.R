## ============================================================
## Assignment 1: data.table
## Precipitation data analysis
## Student: Gildas Pacifique Niyonkuru
## Czech University of Life Sciences Prague
## ============================================================

## load package
library(data.table)

## load data
dta <- readRDS(file = "/home/gildas/Downloads/prec_data.rds")

## convert to data.table
dta_t <- as.data.table(x = dta)


## ============================================================
## PART 2 — BASIC INSPECTION
## ============================================================

## check classes
class(x = dta)
class(x = dta_t)

## first rows
head(x = dta_t)

## size comparison
object.size(x = dta)
object.size(x = dta_t)

## column names
names(x = dta_t)

## dimensions
nrow(x = dta_t)
ncol(x = dta_t)

## structure
str(object = dta_t)

## summary
summary(object = dta_t)

## unique stations and elements
dta_t[, uniqueN(x = STATION)]
dta_t[, uniqueN(x = ELEMENT)]

## check VALUE range including zeros and negatives
dta_t[, .(min = min(x = VALUE, na.rm = TRUE),
          max = max(x = VALUE, na.rm = TRUE),
          n_na = sum(is.na(x = VALUE)),
          n_neg = sum(VALUE < 0, na.rm = TRUE),
          n_zero = sum(VALUE == 0, na.rm = TRUE))]

## check FLAG values
dta_t[, .N, by = FLAG]

## check QUALITY values
dta_t[, .N, by = QUALITY]

## check X column — mostly NA, likely not useful
dta_t[, .N, by = X]

## identifier variables: STATION, ELEMENT, DT
## categorical: STATION, ELEMENT, FLAG
## needs cleaning: FLAG (character NAs), X (all NA), VALUE (negatives?)


## ============================================================
## PART 3 — FILTERING AND SUMMARY STATISTICS
## ============================================================

## filter non-zero values
nz <- dta_t[VALUE > 0, .(STATION, DT, VALUE, FLAG, QUALITY)]

## summary by station
nz[, .(mean = mean(x = VALUE),
       sd = sd(x = VALUE),
       iqr = IQR(x = VALUE),
       min = min(x = VALUE),
       max = max(x = VALUE)),
   by = STATION]

## summary by station and year
nz[, .(mean = mean(x = VALUE),
       sd = sd(x = VALUE),
       iqr = IQR(x = VALUE),
       min = min(x = VALUE),
       max = max(x = VALUE)),
   by = .(STATION, year(x = DT))]


## ============================================================
## PART 4 — DUPLICATE ANALYSIS
## ============================================================

## count exact duplicate rows
sum(duplicated(x = dta_t))

## show examples of exact duplicates
dta_t[duplicated(x = dta_t) | duplicated(x = dta_t, fromLast = TRUE)][1:10]

## logical duplicates: same STATION + DT (should be unique per station per hour)
dt_dups <- dta_t[, .(n = .N),
                 by = .(STATION, DT)]

## count logical duplicates
dt_dups[n > 1, .N]

## show examples
dt_dups[n > 1][1:10]

## stations with most duplicates
dt_dups[n > 1, .(n_dups = .N),
        by = STATION][order(-n_dups)][1:10]

## logical duplicates should be investigated before removal
## same station and timestamp with different VALUE suggests data entry issues


## ============================================================
## PART 5 — CLEANING
## ============================================================

## flag missing VALUE
dta_t[, missing_value := is.na(x = VALUE)]

## flag negative values — suspicious for precipitation
dta_t[, neg_value := VALUE < 0]

## flag suspicious FLAG column entries
dta_t[, has_flag := !is.na(x = FLAG)]

## remove the X column — all NA, not useful
dta_t[, X := NULL]

## remove exact duplicates
dta_t <- unique(x = dta_t)

## check how many rows removed
nrow(x = dta_t)

## flag rows where VALUE is extreme (> 99th percentile)
q99 <- dta_t[VALUE > 0, quantile(x = VALUE,
                                  probs = 0.99,
                                  na.rm = TRUE)]

dta_t[, extreme_value := VALUE > q99]

## add row id within each station
dta_t[, row_id := 1:.N,
      by = STATION]


## ============================================================
## PART 6 — RENAMING AND COLUMN WORK
## ============================================================

## rename QUALITY to quality_code for clarity
setnames(x = dta_t,
         old = c("QUALITY"),
         new = c("quality_code"),
         skip_absent = TRUE)

## rename FLAG to flag_code
setnames(x = dta_t,
         old = c("FLAG"),
         new = c("flag_code"),
         skip_absent = TRUE)

## add year column — useful for time-based analysis
dta_t[, yr := year(x = DT)]

## add month column
dta_t[, mo := month(x = DT)]

## add indicator for missing quality
dta_t[, missing_quality := is.na(x = quality_code) | quality_code == 0]

## remove helper column row_id after use
dta_t[, row_id := NULL]


## ============================================================
## PART 7 — STATION SUMMARY
## ============================================================

## station summary
st_sum <- dta_t[, .(n_rows = .N,
                    mean_val = mean(x = VALUE, na.rm = TRUE),
                    sd_val = sd(x = VALUE, na.rm = TRUE),
                    min_val = min(x = VALUE, na.rm = TRUE),
                    max_val = max(x = VALUE, na.rm = TRUE),
                    n_nonzero = sum(VALUE > 0, na.rm = TRUE),
                    n_missing = sum(missing_value),
                    n_flagged = sum(has_flag, na.rm = TRUE),
                    n_extreme = sum(extreme_value, na.rm = TRUE),
                    n_negative = sum(neg_value, na.rm = TRUE)),
                by = STATION]

## stations with most records
st_sum[order(-n_rows)][1:10]

## stations with highest non-zero precipitation
st_sum[order(-mean_val)][1:10]

## most problematic stations (most flagged + extreme + negative)
st_sum[, problem_score := n_flagged + n_extreme + n_negative]
st_sum[order(-problem_score)][1:10]

## stations with most suspicious rows
st_sum[order(-n_extreme)][1:10]


## ============================================================
## PART 8 — ELEMENT SUMMARY
## ============================================================

## element summary
el_sum <- dta_t[, .(n_rows = .N,
                    n_stations = uniqueN(x = STATION),
                    mean_val = mean(x = VALUE, na.rm = TRUE),
                    sd_val = sd(x = VALUE, na.rm = TRUE),
                    prop_missing = mean(x = is.na(x = VALUE)),
                    n_flagged = sum(has_flag, na.rm = TRUE),
                    n_questionable = sum(quality_code > 0, na.rm = TRUE)),
                by = ELEMENT]

## show element summary
print(x = el_sum)

## SRA1H is the only element — hourly precipitation
## reliability depends on flag and quality distribution


## ============================================================
## PART 10 — MERGE TASK
## ============================================================

## create helper table with zero values over full date sequence
## represents expected hourly observations for a complete time series
dt <- data.table(VALUE = 0,
                 DT = seq(from = as.POSIXct(x = "2018-01-01 00:00:00"),
                          to = as.POSIXct(x = "2026-01-01 00:00:00"),
                          by = "hour"))

## use one station for demonstration
nz_one <- nz[STATION == nz[1, STATION]]

## merge filtered non-zero data with zero sequence
## this fills in the time series gaps with explicit zeros
## useful for identifying missing observation periods
dt_merged <- merge(x = dt,
                   y = nz_one[, .(DT, VALUE)],
                   by = "DT",
                   all.x = TRUE)

## rename merged VALUE columns
setnames(x = dt_merged,
         old = c("VALUE.x", "VALUE.y"),
         new = c("zero_baseline", "observed"),
         skip_absent = TRUE)

## rows where observed is NA = hours with no recorded precipitation
dt_merged[, missing_obs := is.na(x = observed)]

## summary of coverage
dt_merged[, .(total_hours = .N,
              hours_with_data = sum(!missing_obs),
              hours_missing = sum(missing_obs),
              coverage_pct = round(x = mean(!missing_obs) * 100,
                                   digits = 1))]


## ============================================================
## PART 11 — RESHAPE TASK
## ============================================================

## create a small station-year summary for reshaping
dt_wide_prep <- dta_t[VALUE > 0,
                      .(mean_val = round(x = mean(x = VALUE,
                                                   na.rm = TRUE),
                                         digits = 2)),
                      by = .(STATION, yr)]

## take top 5 stations by record count for readability
top5 <- st_sum[order(-n_rows)][1:5, STATION]

dt_wide_prep <- dt_wide_prep[STATION %in% top5]

## reshape to wide: one row per year, one column per station
dt_wide <- dcast(data = dt_wide_prep,
                 formula = yr ~ STATION,
                 value.var = "mean_val")

## show wide format
print(x = dt_wide)

## reshape back to long
dt_long <- melt(data = dt_wide,
                id.vars = "yr",
                variable.name = "STATION",
                value.name = "mean_val")

## show long format
print(x = dt_long)

## reshape helped compare yearly precipitation trends
## across top stations side by side in wide format


## ============================================================
## PART 12 — FINAL DISCUSSION
## ============================================================

## Main data problems found:
## 1. Large number of zero VALUE rows — most hours have no precipitation
## 2. FLAG column is mostly NA but some rows have character flags
## 3. X column was entirely NA — removed
## 4. Some stations have extreme VALUES above 99th percentile
## 5. Logical duplicates exist (same STATION + DT combination)
## 6. quality_code of 0 is ambiguous — could mean good or missing

## Cleaning decisions:
## - Removed X column (no information)
## - Removed exact duplicates using unique()
## - Flagged negatives, extremes, and missing values with := 
## - Renamed FLAG and QUALITY for clarity

## Uncertain decisions:
## - Zero values could be true zeros or missing — kept but flagged
## - quality_code == 0 treated as potentially missing quality
## - Extreme value threshold (99th percentile) is arbitrary

## Most suspicious: stations with highest n_extreme and n_negative
## Most useful data.table operations: := for flagging, grouping with by,
## dcast/melt for reshaping, merge for time series gap analysis

## show final object sizes
cat("dta_t rows after cleaning:", nrow(x = dta_t), "\n")
cat("st_sum rows:", nrow(x = st_sum), "\n")
cat("el_sum rows:", nrow(x = el_sum), "\n")
