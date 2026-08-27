#!/usr/bin/env python3
"""Safely add RELATIONS to test cases."""
import json
import re

def load_mappings():
    with open('/home/runner/work/gentlebeam-sk/gentlebeam-sk/tc_rq_mappings.json') as f:
        return json.load(f)

def process_file(filepath, tc_list):
    """Add RELATIONS to test cases."""
    tc_to_rq = {item['test_case_uid']: item['requirement_uids'] for item in tc_list}
    
    with open(filepath, 'r') as f:
        lines = f.readlines()
    
    output = []
    i = 0
    
    while i < len(lines):
        output.append(lines[i])
        
        # Found a TEST_CASE block
        if lines[i].strip() == '[TEST_CASE]':
            # Look for UID on next line
            if i + 1 < len(lines) and lines[i + 1].startswith('UID:'):
                uid = lines[i + 1].split(':', 1)[1].strip()
                output.append(lines[i + 1])
                i += 2
                
                # Now find the end of this test case and add RELATIONS if needed
                found_expected = False
                found_closing = False
                
                while i < len(lines):
                    output.append(lines[i])
                    
                    if 'EXPECTED_BEHAVIOR:' in lines[i]:
                        found_expected = True
                    
                    # Look for closing <<<
                    if found_expected and lines[i].strip() == '<<<':
                        found_closing = True
                        i += 1
                        
                        # Now add RELATIONS if needed
                        if uid in tc_to_rq:
                            # Check if RELATIONS already exists
                            has_relations = False
                            for j in range(i, min(i + 10, len(lines))):
                                if 'RELATIONS:' in lines[j]:
                                    has_relations = True
                                    break
                                if lines[j].strip() and not lines[j].startswith(' ') and lines[j][0] not in '- ':
                                    # Non-indented line that's not a list item
                                    if lines[j].strip() not in ['', '[TEST_CASE]', '[[/SECTION]]']:
                                        break
                            
                            if not has_relations:
                                # Add RELATIONS
                                output.append('RELATIONS:\n')
                                for rq in tc_to_rq[uid]:
                                    output.append('- TYPE: Parent\n')
                                    output.append(f'  VALUE: {rq}\n')
                                    output.append('  ROLE: Verifies\n')
                        break
                    
                    i += 1
                
                if not found_closing:
                    # Something went wrong, but continue
                    pass
                continue
        
        i += 1
    
    return ''.join(output)

def main():
    mappings = load_mappings()
    
    # Process external
    ext_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/cnc/voxelray-gentlebeamcnc/docs/external/main_test_protocol.sdoc'
    print("Processing external...")
    result = process_file(ext_file, mappings['external'])
    with open(ext_file, 'w') as f:
        f.write(result)
    ext_count = result.count('RELATIONS:')
    print(f"  Added {ext_count} RELATIONS")
    
    # Process internal
    int_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/cnc/voxelray-gentlebeamcnc/docs/internal/main_test_protocol.sdoc'
    print("Processing internal...")
    result = process_file(int_file, mappings['internal'])
    with open(int_file, 'w') as f:
        f.write(result)
    int_count = result.count('RELATIONS:')
    print(f"  Added {int_count} RELATIONS")

if __name__ == '__main__':
    main()
