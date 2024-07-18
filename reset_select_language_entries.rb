# frozen_string_literal: true

require "json"

language_files = Dir["ValheimFortress/Localizations/*"]
keys_to_remove = %w[wild_shrine_mistlands_request wild_shrine_plains_request wild_shrine_mountain_request wild_shrine_swamp_request wild_shrine_blackforest_request wild_shrine_green_request]

language_files.each do |lang_file|
  next if lang_file == "ValheimFortress/Localizations/English.json"

  lang_json = JSON.parse(File.read(lang_file.to_s))
  puts "Removing keys from #{lang_file}"
  keys_to_remove.each do |rm_key|
    lang_json.delete(rm_key)
  end
  File.open(lang_file.to_s, "w") { |f| f.write(JSON.pretty_generate(lang_json)) }
end
